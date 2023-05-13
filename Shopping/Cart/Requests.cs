using Shopping.Cart.Core;
using Shopping.Core;
using Shopping.Product;

namespace Shopping.Cart.Requests;

public record UpdateCartItemRequest(CustomerId CustomerId, CartId CartId, Sku Sku, uint Quantity);
public record UpdateCartItemResponse(CartId CartId, CorrelationId CorrelationId);

public record RemoveItemFromCartRequest(CustomerId CustomerId, CartId CartId, Sku Sku);

public record RemoveItemFromCartResponse(CartId CartId, CorrelationId CorrelationId);

public record AddToCartRequest(CustomerId CustomerId, CartId CartId, Sku Sku, uint Quantity);

public record AddToCartResponse(CartId CartId, CorrelationId CorrelationId);