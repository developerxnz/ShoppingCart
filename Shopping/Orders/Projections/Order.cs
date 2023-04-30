using Shopping.Core;
using Shopping.Orders.Core;

namespace Shopping.Orders.Projections;

public record Order(
    OrderId OrderId, 
    DateTime CreatedOnUtc,
    Tax Tax,
    Total Total
    );