using Shopping.Domain.Cart.Core;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Domain.Cart.Requests;

public record UpdateCartItemRequest(CustomerId CustomerId, CartId CartId, Sku Sku, CartQuantity Quantity);

public record UpdateCartItemResponse(CartId CartId, CorrelationId CorrelationId);

public record RemoveItemFromCartRequest(CustomerId CustomerId, CartId CartId, Sku Sku);

public record RemoveItemFromCartResponse(CartId CartId, CorrelationId CorrelationId);

public record AddToCartRequest(CustomerId CustomerId, CartId CartId, Sku Sku, CartQuantity Quantity);

public record AddToCartResponse(CartId CartId, CorrelationId CorrelationId);