using Shopping.Product;

namespace Shopping.Cart.Projections;

public record ProductDescription(string Description);

public record CartItem(
    DateTime CreatedOnUtc, 
    CartId CartId, 
    Sku Sku, 
    ProductDescription ProductDescription, 
    uint Quantity);