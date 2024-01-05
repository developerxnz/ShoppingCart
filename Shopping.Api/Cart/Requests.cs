namespace Shopping.Api.Cart.Requests;

public record Requests(Guid CustomerId, string Sku, uint Quantity);