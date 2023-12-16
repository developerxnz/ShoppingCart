using Shopping.Domain.Cart.Projections;

namespace Shopping.Domain.Product.Core;

public record ProductId(Guid Value);

public record Sku
{
    public string Value { get; private set; }
    
    public Sku(string value)
    {
        Value = value;
    }
};

public record Description(string Value);

public record ProductPrice(Decimal Amount);

public record Product(ProductId ProductId, Sku Sku, ProductDescription Description);