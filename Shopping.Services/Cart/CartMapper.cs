using ErrorOr;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Cart.Events;
using Shopping.Domain.Core;
using Shopping.Domain.Core.Persistence;
using Shopping.Infrastructure.Persistence.Cart;
using Shopping.Services.Interfaces;
using CartEvent = Shopping.Infrastructure.Persistence.Cart.CartEvent;
using CartItem = Shopping.Domain.Cart.Core.CartItem;
using CartItemAddedEvent = Shopping.Domain.Cart.Events.CartItemAddedEvent;
using CartItemRemovedEvent = Shopping.Domain.Cart.Events.CartItemRemovedEvent;
using CartItemUpdatedEvent = Shopping.Domain.Cart.Events.CartItemUpdatedEvent;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Services.Cart;

public sealed class CartMapper :
    Mapper<Domain.Cart.CartAggregate, Infrastructure.Persistence.Cart.CartAggregate, ICartEvent, CartEvent>,
    IMapper<Domain.Cart.CartAggregate, Infrastructure.Persistence.Cart.CartAggregate, ICartEvent, CartEvent>
{
    public override Infrastructure.Persistence.Cart.CartAggregate FromDomain(Domain.Cart.CartAggregate aggregate)
    {
        return new Infrastructure.Persistence.Cart.CartAggregate
        {
            Id = aggregate.Id.Value.ToString(),
            CustomerId = aggregate.CustomerId.Value.ToString(),
            CreatedOnUtc = aggregate.CreatedOnUtc,
            Etag = aggregate.Etag,
            Items = Enumerable.Empty<Infrastructure.Persistence.Cart.CartItem>(),
            Metadata = new Metadata(
                aggregate.MetaData.StreamId.Value.ToString(),
                aggregate.MetaData.Version.Value,
                aggregate.MetaData.TimeStamp
            )
        };
    }

    public override CartEvent FromDomain(ICartEvent @event)
        => @event switch
        {
            CartItemAddedEvent cartItemAddedEvent => cartItemAddedEvent.FromDomain(),
            CartItemRemovedEvent cartItemRemovedEvent => cartItemRemovedEvent.FromDomain(),
            CartItemUpdatedEvent cartItemUpdatedEvent => cartItemUpdatedEvent.FromDomain(),
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };

    public (Infrastructure.Persistence.Cart.CartAggregate, IEnumerable<CartEvent>) FromDomain(Domain.Cart.CartAggregate aggregate, IEnumerable<IEvent> events)
    {
        throw new NotImplementedException();
    }

    public (Infrastructure.Persistence.Cart.CartAggregate, IEnumerable<CartEvent>)
        FromDomain(Domain.Cart.CartAggregate aggregate, IEnumerable<ICartEvent> events)
    {
        var aggregateDto = FromDomain(aggregate);
        var eventDtos = events.Select(FromDomain).ToList();
        
        return (aggregateDto, eventDtos);
    }

    public override ErrorOr<Domain.Cart.CartAggregate> ToDomain(Infrastructure.Persistence.Cart.CartAggregate dto)
    {
        if (!Guid.TryParse(dto.CustomerId, out Guid customerId))
        {
            return Error.Validation("Invalid CustomerId");
        }

        if (!Guid.TryParse(dto.Id, out Guid cartId))
        {
            return Error.Validation("Invalid CustomerId");
        }

        return new Domain.Cart.CartAggregate(dto.CreatedOnUtc, new CustomerId(customerId))
        {
            Id = new CartId(cartId),
            Etag = dto.Etag,
            Items = Enumerable.Empty<CartItem>(),
            MetaData = new MetaData(new StreamId(cartId), new Version(dto.Metadata.Version), dto.Metadata.Timestamp)
        };
    }
    
    public override ErrorOr<ICartEvent> ToDomain(CartEvent @event)
    => @event switch
        {
            Infrastructure.Persistence.Cart.CartItemAddedEvent cartItemAddedEvent =>
                 cartItemAddedEvent.ToDomain(),
            Infrastructure.Persistence.Cart.CartItemRemovedEvent cartItemRemovedEvent =>
                cartItemRemovedEvent.ToDomain(),
            Infrastructure.Persistence.Cart.CartItemUpdatedEvent cartItemUpdatedEvent =>
                cartItemUpdatedEvent.ToDomain(),
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };
}