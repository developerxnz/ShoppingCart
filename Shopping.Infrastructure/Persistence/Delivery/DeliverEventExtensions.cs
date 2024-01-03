using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Orders.Core;

namespace Shopping.Infrastructure.Persistence.Delivery;

public static class DeliverEventExtensions
{
    public static DeliveryCreatedEvent FromDomain(this Domain.Delivery.Events.DeliveryCreatedEvent domain)
    {
        return new DeliveryCreatedEvent(
            domain.CreatedOnUtc,
            domain.CustomerId.Value.ToString(),
            domain.OrderId.Value.ToString(),
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static Domain.Delivery.Events.DeliveryCreatedEvent ToDomain(this DeliveryCreatedEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        OrderId orderId = new OrderId(Guid.Parse(dto.OrderId));
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        Domain.Core.Version version = new(dto.Version);

        return new Domain.Delivery.Events.DeliveryCreatedEvent(
            dto.CreatedOnUtc, 
            customerId, 
            orderId, 
            version,
            correlationId, causationId);
    }
    
    public static DeliveryCompletedEvent FromDomain(this Domain.Delivery.Events.DeliveryCompletedEvent domain)
    {
        return new DeliveryCompletedEvent(
            domain.CompletedOnUtc,
            domain.CustomerId.Value.ToString(),
            domain.DeliveryId.ToString(),
            domain.OrderId.Value.ToString(),
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static Domain.Delivery.Events.DeliveryCompletedEvent ToDomain(this DeliveryCompletedEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        OrderId orderId = new OrderId(Guid.Parse(dto.OrderId));
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        Domain.Core.Version version = new(dto.Version);
        DeliveryId deliveryId = new DeliveryId(Guid.Parse(dto.DeliveryId));

        return new Domain.Delivery.Events.DeliveryCompletedEvent(
            dto.CompletedOnUtc, 
            customerId, 
            deliveryId,
            orderId,
            version,
            correlationId, causationId);
    }
    
    public static DeliveryCancelledEvent FromDomain(this Domain.Delivery.Events.DeliveryCancelledEvent domain)
    {
        return new DeliveryCancelledEvent(
            domain.CancelledOnUtc,
            domain.CustomerId.Value.ToString(),
            domain.OrderId.Value.ToString(),
            domain.DeliveryId.ToString(),
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static Domain.Delivery.Events.DeliveryCancelledEvent ToDomain(this DeliveryCancelledEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        OrderId orderId = new OrderId(Guid.Parse(dto.OrderId));
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        Domain.Core.Version version = new(dto.Version);
        DeliveryId deliveryId = new DeliveryId(Guid.Parse(dto.DeliveryId));
        
        return new Domain.Delivery.Events.DeliveryCancelledEvent(
            dto.CancelledOnUtc, 
            customerId, 
            deliveryId,
            orderId, 
            version,
            correlationId, causationId);
    }
}