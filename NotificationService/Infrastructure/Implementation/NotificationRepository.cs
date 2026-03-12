using Application.Interfaces;
using NotificationService.Domain;
using Infrastructure.Data;

namespace Infrastructure.Implementation;

public class NotificationRepository(NotificationDbContext dbContext) : BaseRepository<Notification>(dbContext), INotificationRepository;
