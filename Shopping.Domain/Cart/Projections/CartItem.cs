using Shopping.Domain.Cart.Core;
using Shopping.Domain.Product.Core;
using Shopping.Domain.Product;

namespace Shopping.Domain.Cart.Projections;

public record ProductDescription(string Description);

public record CartItem(
    DateTime CreatedOnUtc, 
    CartId CartId, 
    Sku Sku, 
    ProductDescription ProductDescription, 
    uint Quantity);