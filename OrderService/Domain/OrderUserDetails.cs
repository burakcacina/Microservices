namespace Domain;

public class OrderUserDetails
{
    public int Id { get; set; }
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string PhoneNumber { get; init; } = null!;
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
