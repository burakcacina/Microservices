using Microsoft.EntityFrameworkCore;
using Application.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Implementation;

public class BaseRepository<TEntity>(NotificationDbContext dbContext) : IBaseRepository<TEntity> where TEntity : class
{
    private readonly NotificationDbContext _dbContext = dbContext;
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

    public async Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default)
    {
        return await Set.FindAsync(keyValues, cancellationToken);
    }
}
