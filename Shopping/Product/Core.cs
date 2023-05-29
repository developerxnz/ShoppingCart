namespace Shopping.Product;

public record ProductId(Guid Value);

public record Sku(Guid Value);

public record Description(string Value);

public record ProductPrice(Decimal Amount);