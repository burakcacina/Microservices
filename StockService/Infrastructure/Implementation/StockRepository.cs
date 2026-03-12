using Application.Interfaces;
using Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation;

public class StockRepository : BaseRepository<Stock>, IStockRepository
{
    private readonly StockDbContext _dbContext;

    public StockRepository(StockDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Stock?> GetByProductIdAsync(int productId, CancellationToken ct = default)
    {
        return _dbContext.Products.Where(p => p.Id == productId)
            .Select(p => p.Stock)
            .FirstOrDefaultAsync(ct);
    }
}
