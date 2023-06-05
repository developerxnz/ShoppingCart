using Shopping.Cart.Core;
using Shopping.Product;
using Shopping.Product.Core;

namespace Shopping.Cart.Projections;

public record ProductDescription(string Description);

public record CartItem(
    DateTime CreatedOnUtc, 
    CartId CartId, 
    Sku Sku, 
    ProductDescription ProductDescription, 
    uint Quantity);