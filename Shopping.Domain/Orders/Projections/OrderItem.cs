using Shopping.Domain.Orders.Core;

namespace Shopping.Domain.Orders.Projections;

public record OrderItem(
    OrderId OrderId
    );