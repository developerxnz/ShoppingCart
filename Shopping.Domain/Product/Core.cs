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

public record Events(ProductId ProductId, Sku Sku, ProductDescription Description);

public record ProductDescription(string Description);

public record CartQuantity
{
    public uint Value { get; init; }

    public CartQuantity(uint quantity)
    {
        switch (quantity)
        {
            case > 100: throw new Exception("Quantity cannot be greater than 100");
            case 0: throw new Exception("Quantity cannot be 0");
        }

        Value = quantity;
    }
}