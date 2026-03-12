using Domain;
using FluentValidation;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Application.Models.Request;
using Application.Models.Response;
using System.Text.Json;
using System.Net;

namespace Application.Services
{

    public class OrderService(
        IOrderRepository orderRepository,
        IOutbox outbox,
        IUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        IValidator<CreateOrderRequest> validator,
        ILogger<OrderService> logger) : IOrderService
    {
        public async Task<IResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct)
        {
            try
            {
                logger.LogInformation("Create Order Request received");

                var validationResult = await validator.ValidateAsync(request, ct);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
                }

                var stockClient = httpClientFactory.CreateClient("StockService");
                var stockResponse = await stockClient.GetAsync($"api/products/{request.ProductId}", ct);

                if (stockResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return Results.BadRequest($"Stock not found for product id: {request.ProductId}");
                }

                if (!stockResponse.IsSuccessStatusCode)
                {
                    logger.LogWarning("Stock service check failed for product {ProductId}. Status: {StatusCode}", request.ProductId, stockResponse.StatusCode);
                    return Results.StatusCode((int)HttpStatusCode.BadGateway);
                }

                await using var responseStream = await stockResponse.Content.ReadAsStreamAsync(ct);
                var stockData = await JsonSerializer.DeserializeAsync<StockProductResponse>(
                    responseStream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    ct);
                var availableStock = stockData?.Stock ?? throw new InvalidOperationException("Invalid stock response payload.");

                if (availableStock < request.Quantity) return Results.BadRequest($"Insufficient stock for product id: {request.ProductId}. Requested: {request.Quantity}, Available: {availableStock}");

                await unitOfWork.BeginTransactionAsync(ct);

                var order = new Order
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    User = new OrderUserDetails
                    {
                        Name = request.User.Name,
                        Email = request.User.Email,
                        PhoneNumber = request.User.PhoneNumber
                    }
                };

                await orderRepository.AddAsync(order, ct);

                var evt = new
                {
                    Order = new
                    {
                        order.Id,
                        order.ProductId,
                        order.Quantity,
                        order.OrderedAt,
                        User = new
                        {
                            order.User.Name,
                            order.User.Email,
                            order.User.PhoneNumber
                        }
                    }
                };

                var payloadJson = JsonSerializer.Serialize(evt);
                await outbox.AddAsync(new Domain.Outbox
                {
                    Id = Guid.NewGuid(),
                    Type = "order.created",
                    Payload = payloadJson,
                    OccurredAt = DateTimeOffset.UtcNow,
                    Attempts = 0
                }, ct);
                await unitOfWork.SaveChangesAsync(ct);
                await unitOfWork.CommitTransactionAsync(ct);

                logger.LogInformation("Successfully Created Order and Saved into outbox ");

                return Results.Ok("Order created successfully with id: " + order.Id);
            }
            catch (System.Exception)
            {
                await unitOfWork.RollbackTransactionAsync(ct);
                logger.LogError("Error occurred while creating order");
                throw;
            }
        }
    }
}
