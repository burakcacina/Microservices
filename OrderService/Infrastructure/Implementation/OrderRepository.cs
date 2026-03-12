using Application.Interfaces;
using Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implementation;

public class OrderRepository(OrderDbContext dbContext) : BaseRepository<Order>(dbContext), IOrderRepository
{
    public override IQueryable<Order> GetAll()
        => Set.AsNoTracking()
            .Include(x => x.User);
}
