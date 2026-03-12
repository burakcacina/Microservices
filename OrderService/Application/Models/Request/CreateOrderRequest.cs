namespace Application.Models.Request;

public sealed record CreateOrderRequest(
    int ProductId,
    int Quantity,
    CreateUserRequest User);

public sealed record CreateUserRequest(string Name, string Email, string PhoneNumber);
