using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation;

public class BaseRepository<TEntity>(StockDbContext dbContext) : IBaseRepository<TEntity> where TEntity : class
{
    private readonly StockDbContext _dbContext = dbContext;
    public DbSet<TEntity> Set => _dbContext.Set<TEntity>();

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity) => Set.Update(entity);

    public void Remove(TEntity entity) => Set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public virtual IQueryable<TEntity> GetAll()
    {
        return Set.AsNoTracking();
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Set.FindAsync(id, cancellationToken);
    }
}
