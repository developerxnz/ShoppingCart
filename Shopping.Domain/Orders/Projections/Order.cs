using Shopping.Domain.Core;
using Shopping.Domain.Orders.Core;

namespace Shopping.Domain.Orders.Projections;

public record Order(
    OrderId OrderId, 
    DateTime CreatedOnUtc,
    Tax Tax,
    Total Total
    );