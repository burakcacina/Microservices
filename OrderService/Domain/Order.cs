namespace Domain;

public class Order
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime? OrderedAt { get; init; } = DateTime.UtcNow;
    public OrderUserDetails User { get; set; } = null!;
}
