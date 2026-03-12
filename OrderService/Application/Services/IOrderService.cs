using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Application.Models.Request;

namespace Application.Services
{
    public interface IOrderService
    {
        Task<IResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct);
    }
}
