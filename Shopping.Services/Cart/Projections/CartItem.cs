using Shopping.Domain.Cart.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Services.Cart.Projections;

public record CartItem(
    DateTime CreatedOnUtc, 
    CartId CartId, 
    Sku Sku, 
    ProductDescription ProductDescription, 
    uint Quantity);