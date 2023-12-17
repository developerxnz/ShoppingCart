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

public record ProductDescription(string Description);

public record CartQuantity
{
    public uint Value { get; init; }
    public static ErrorOr.ErrorOr<CartQuantity> Create(uint quantity)
    {
        return quantity switch
        {
            > 100 => ErrorOr.Error.Validation("", "Quantity cannot be greater than 100"),
            0 => ErrorOr.Error.Validation("", "Quantity cannot be 0"),
            _ => new CartQuantity() {Value = quantity}
        };
    }
}