using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Application.Interfaces;
using NotificationService.Domain;
using Infrastructure.Models;
using Infrastructure.Util;

namespace Infrastructure.HostedService;

public sealed class RabbitMqConsumerHostedService(
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<RabbitMqConsumerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = configuration["RabbitMq:Host"];
        var port = configuration["RabbitMq:Port"];
        var username = configuration["RabbitMq:Username"];
        var password = configuration["RabbitMq:Password"];
        var exchange = configuration["RabbitMq:Exchange"];
        var exchangeType = configuration["RabbitMq:ExchangeType"] ?? ExchangeType.Fanout;
        var queue = configuration["RabbitMq:Queue"];

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = int.TryParse(port, out var p) ? p : 5672,
            UserName = username,
            Password = password,
            DispatchConsumersAsync = true
        };

        var retryDelay = TimeSpan.FromSeconds(2);
        var maxRetryDelay = TimeSpan.FromSeconds(30);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = factory.CreateConnection(clientProvidedName: "notificationservice.consumer");
                using var channel = connection.CreateModel();

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                channel.ExchangeDeclare(exchange: exchange, type: exchangeType, durable: true);
                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                channel.QueueBind(queue: queue, exchange: exchange, routingKey: string.Empty);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    try
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        var type = ea.BasicProperties?.Type;
                        var IdempotencyKey = Helper.HeaderAsString(ea.BasicProperties, "Idempotency-Key") ?? Guid.NewGuid().ToString();

                        logger.LogInformation("Notification Service received message. Type: '{Type}', IdempotencyKey: '{IdempotencyKey}'", type, IdempotencyKey);
                        using var scope = scopeFactory.CreateScope();
                        var dispatcher = scope.ServiceProvider.GetRequiredService<INotificationDispatcher>();

                        var payload = JsonSerializer.Deserialize<OrderCreatedPayload>(message);
                        var user = payload?.Order?.User ?? throw new InvalidOperationException("Invalid order payload. Missing 'Order.User' object.");
                        var emailRecipient = string.IsNullOrWhiteSpace(user.Email) ? throw new InvalidOperationException("Invalid order payload. Missing 'Order.User.Email'.") : user.Email;
                        var smsRecipient = string.IsNullOrWhiteSpace(user.PhoneNumber) ? throw new InvalidOperationException("Invalid order payload. Missing 'Order.User.PhoneNumber'.") : user.PhoneNumber;
                        var notificationText = $"Thank you for ordering. Your order {payload?.Order?.Id} has been received and is being processed.";

                        var recipients = new[]
                        {
                            (Channel: NotificationChannel.Email, Recipient: emailRecipient),
                            (Channel: NotificationChannel.Sms, Recipient: smsRecipient)
                        };

                        foreach (var recipient in recipients)
                        {
                            await dispatcher.DispatchAsync(new Notification
                            {
                                Channel = recipient.Channel,
                                Recipient = recipient.Recipient,
                                Message = notificationText,
                                IdempotencyKey = IdempotencyKey
                            }, stoppingToken);
                        }

                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing message.");
                        channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                    }
                };

                channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "RabbitMQ connection/consume failed. Retrying in {RetrySeconds:0}s...", retryDelay.TotalSeconds);
                await Task.Delay(retryDelay, stoppingToken);
                retryDelay = TimeSpan.FromSeconds(Math.Min(maxRetryDelay.TotalSeconds, retryDelay.TotalSeconds * 2));
            }
        }
    }
}
