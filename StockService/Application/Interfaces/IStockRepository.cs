using Domain;

namespace Application.Interfaces
{
    public interface IStockRepository : IBaseRepository<Stock>
    {
        public Task<Stock?> GetByProductIdAsync(int productId, CancellationToken ct = default);
    }
}
