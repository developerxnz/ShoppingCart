using Shopping.Domain.Core;
using Shopping.Domain.Orders.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Infrastructure.Persistence.Orders;

public static class OrderEventExtensions
{
    public static Domain.Orders.Events.OrderCreatedEvent ToDomain(this Events.OrderCreatedEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        Version version = new Version(dto.Version);
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));

        return new Domain.Orders.Events.OrderCreatedEvent(
            dto.CreatedOnUtc,
            customerId,
            version,
            correlationId,
            causationId
        );
    }

    public static Events.OrderCreatedEvent FromDomain(this Domain.Orders.Events.OrderCreatedEvent domain)
    {
        return new Events.OrderCreatedEvent(
            domain.Id.Value.ToString(),
            domain.CreatedOnUtc,
            domain.CustomerId.Value.ToString(),
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }
    
    public static Domain.Orders.Events.OrderCancelledEvent ToDomain(this Events.OrderCancelledEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        Version version = new Version(dto.Version);
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        OrderId orderId = new OrderId(Guid.Parse(dto.OrderId));
        
        return new Domain.Orders.Events.OrderCancelledEvent(
            dto.CancelledOnUtc,
            customerId,
            orderId,
            version,
            correlationId,
            causationId
        );
    }

    public static Events.OrderCancelledEvent FromDomain(this Domain.Orders.Events.OrderCancelledEvent domain)
    {
        return new Events.OrderCancelledEvent(
            domain.Id.Value.ToString(),
            domain.CancelledOnUtc,
            domain.CustomerId.Value.ToString(),
            domain.OrderId.Value.ToString(),
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }
    
    public static Domain.Orders.Events.OrderCompletedEvent ToDomain(this Events.OrderCompletedEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        Version version = new Version(dto.Version);
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        OrderId orderId = new OrderId(Guid.Parse(dto.OrderId));
        
        return new Domain.Orders.Events.OrderCompletedEvent(
            dto.CompletedOnUtc,
            customerId,
            orderId,
            version,
            correlationId,
            causationId
        );
    }

    public static Events.OrderCompletedEvent FromDomain(this Domain.Orders.Events.OrderCompletedEvent domain)
    {
        return new Events.OrderCompletedEvent(
            domain.Id.Value.ToString(),
            domain.CompletedOnUtc,
            domain.CustomerId.Value.ToString(),
            domain.OrderId.Value.ToString(),
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }
    
}