namespace Shopping.Domain.Orders.Core;

public record OrderId(Guid Value)
{
    public static OrderId Create()
    {
        return new OrderId(Guid.NewGuid());
    }
}