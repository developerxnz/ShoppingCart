namespace Shopping.Product.Core;

public record ProductId(Guid Value);

public record Sku(string Value);

public record Description(string Value);

public record ProductPrice(Decimal Amount);