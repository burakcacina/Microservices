using Application.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Implementation;

public sealed class Outbox(OrderDbContext dbContext) : BaseRepository<Domain.Outbox>(dbContext), IOutbox;
