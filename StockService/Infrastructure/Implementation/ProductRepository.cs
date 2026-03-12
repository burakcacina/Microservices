using Application.Interfaces;
using Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation;

public class ProductRepository(StockDbContext dbContext) : BaseRepository<Product>(dbContext), IProductRepository
{
    public new IQueryable<Product> GetAll()
    {
        return Set.Include(p => p.Stock).OrderBy(p => p.Id).AsNoTracking();
    }

    public new Task<Product?> GetByIdAsync(int productId, CancellationToken ct)
    {
        return Set.Include(p => p.Stock).FirstOrDefaultAsync(p => p.Id == productId, ct);
    }
}
