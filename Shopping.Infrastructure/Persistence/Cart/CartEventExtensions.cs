using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Infrastructure.Persistence.Cart;

public static class CartEventExtensions
{
    public static Domain.Cart.Events.CartItemAddedEvent ToDomain(this CartItemAddedEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        Sku sku = new Sku(dto.Sku);
        CartQuantity quantity = new CartQuantity(dto.Quantity);
        Version version = new Version(dto.Version);
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));

        return new Domain.Cart.Events.CartItemAddedEvent(
            dto.AddedOnUtc,
            customerId,
            sku,
            quantity,
            version,
            correlationId,
            causationId
        );
    }

    public static CartItemAddedEvent FromDomain(this Domain.Cart.Events.CartItemAddedEvent domain)
    {
        return new CartItemAddedEvent(
            domain.Id.Value.ToString(),
            domain.CustomerId.Value.ToString(),
            domain.AddedOnUtc,
            domain.Sku.Value,
            domain.Quantity.Value,
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static CartItemRemovedEvent FromDomain(this Domain.Cart.Events.CartItemRemovedEvent domain)
    {
        return new CartItemRemovedEvent(
            domain.Id.Value.ToString(),
            domain.CustomerId.Value.ToString(),
            domain.RemovedOnUtc,
            domain.Sku.Value,
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static Domain.Cart.Events.CartItemRemovedEvent ToDomain(this CartItemRemovedEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        Sku sku = new Sku(dto.Sku);
        Version version = new Version(dto.Version);
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));

        return new Domain.Cart.Events.CartItemRemovedEvent(
            dto.RemovedOnUtc,
            customerId,
            sku,
            version,
            correlationId,
            causationId
        );
    }
    
    public static CartItemUpdatedEvent FromDomain(this Domain.Cart.Events.CartItemUpdatedEvent domain)
    {
        return new CartItemUpdatedEvent(
            domain.Id.Value.ToString(),
            domain.CustomerId.Value.ToString(),
            domain.UpdatedOnUtc,
            domain.Sku.Value,
            domain.Quantity.Value,
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static Domain.Cart.Events.CartItemUpdatedEvent ToDomain(this CartItemUpdatedEvent dto)
    {
        CustomerId customerId = new CustomerId(Guid.Parse(dto.CustomerId));
        Sku sku = new Sku(dto.Sku);
        Version version = new Version(dto.Version);
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        CartQuantity quantity = new CartQuantity(dto.Quantity);
        return new Domain.Cart.Events.CartItemUpdatedEvent(
            dto.UpdatedOnUtc,
            customerId,
            sku,
            quantity,
            version,
            correlationId,
            causationId
        );
    }
}