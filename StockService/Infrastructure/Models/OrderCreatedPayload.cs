namespace Infrastructure.Models;

public sealed class OrderCreatedPayload
{
    public OrderData? Order { get; set; }
}

public sealed class OrderData
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
