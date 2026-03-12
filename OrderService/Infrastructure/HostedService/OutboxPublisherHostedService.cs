using System.Text;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Infrastructure.HostedService;

public sealed class OutboxPublisherHostedService(
	IServiceProvider serviceProvider,
	IConfiguration configuration,
	ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var publishIntervalSeconds = configuration.GetValue("Outbox:PublishIntervalSeconds", 10);
		var publishInterval = TimeSpan.FromSeconds(Math.Max(1, publishIntervalSeconds));

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await PublishBatchAsync(stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogWarning(ex, "Outbox publisher loop failed.");
			}

			await Task.Delay(publishInterval, stoppingToken);
		}
	}

	private async Task PublishBatchAsync(CancellationToken ct)
	{
		var batchSize = configuration.GetValue("Outbox:BatchSize", 25);
		var maxAttempts = configuration.GetValue("Outbox:MaxAttempts", 10);

		await using var scope = serviceProvider.CreateAsyncScope();
		var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

		var pending = await db.OutboxMessages
			.Where(x => x.ProcessedAt == null && x.Attempts < maxAttempts)
			.OrderBy(x => x.OccurredAt)
			.Take(batchSize)
			.ToListAsync(ct);

		if (pending.Count == 0)
		{
			return;
		}

		using var connection = CreateRabbitConnection(configuration);
		using var channel = connection.CreateModel();

		var exchange = configuration["RabbitMq:Exchange"];
		var exchangeType = configuration["RabbitMq:ExchangeType"] ?? ExchangeType.Fanout;

		channel.ExchangeDeclare(exchange: exchange, type: exchangeType, durable: true, autoDelete: false, arguments: null);
		foreach (var msg in pending)
		{
			try
			{
				var body = Encoding.UTF8.GetBytes(msg.Payload);
				var props = channel.CreateBasicProperties();
				props.ContentType = "application/json";
				props.DeliveryMode = 2;
				props.Persistent = true;
				props.Type = msg.Type;
				props.Headers = new Dictionary<string, object>
				{
					{ "Idempotency-Key", msg.Id.ToString() }
				};

				channel.BasicPublish(exchange: exchange, routingKey: "", basicProperties: props, body: body);
				msg.ProcessedAt = DateTimeOffset.UtcNow;
				msg.LastError = null;
				logger.LogInformation("Published outbox message {Id} (type={Type}).", msg.Id, msg.Type);
			}
			catch (Exception ex)
			{
				msg.Attempts += 1;
				msg.LastError = ex.Message;
				logger.LogWarning(ex, "Failed publishing outbox message {Id} (type={Type}).", msg.Id, msg.Type);

				if (msg.Attempts >= maxAttempts)
				{
					logger.LogError(
						"Outbox message {Id} (type={Type}) reached max attempts ({MaxAttempts}) and will no longer be retried.",
						msg.Id,
						msg.Type,
						maxAttempts);
				}
			}
		}
		await db.SaveChangesAsync(ct);
	}

	private static IConnection CreateRabbitConnection(IConfiguration configuration)
	{
		var host = configuration["RabbitMq:Host"];
		var port = int.TryParse(configuration["RabbitMq:Port"], out var p) ? p : 5672;
		var username = configuration["RabbitMq:Username"];
		var password = configuration["RabbitMq:Password"];

		var factory = new ConnectionFactory
		{
			HostName = host,
			Port = port,
			UserName = username,
			Password = password
		};

		return factory.CreateConnection(clientProvidedName: "orderservice.outbox.publisher");
	}
}
