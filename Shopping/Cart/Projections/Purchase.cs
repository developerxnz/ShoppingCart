using Shopping.Orders.Core;
using Shopping.Product;

namespace Shopping.Cart.Projections;

public record Purchase(DateTime CreatedOnUtc, OrderId OrderId, Sku Sku, uint Quantity);