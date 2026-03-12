namespace Infrastructure.Models;

public sealed class OrderCreatedPayload
{
    public OrderData? Order { get; set; }
}

public sealed class OrderData
{
    public int Id { get; set; }
    public UserData? User { get; set; }
}

public sealed class UserData
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
