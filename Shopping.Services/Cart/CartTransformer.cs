using ErrorOr;
using Shopping.Domain.Cart;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Services.Cart;

public sealed class CartTransformer : Transformer<CartAggregate, Infrastructure.Persistence.Cart.Cart>, ITransformer<CartAggregate, Infrastructure.Persistence.Cart.Cart>
{
    private readonly CartItemTransformer _cartItemTransformer;

    public CartTransformer(CartItemTransformer cartItemTransformer)
    {
        _cartItemTransformer = cartItemTransformer;
    }

    public override Infrastructure.Persistence.Cart.Cart FromDomain(CartAggregate aggregate)
    {
        return new Infrastructure.Persistence.Cart.Cart
        {
            Id = aggregate.Id.Value.ToString(),
            CustomerId = aggregate.CustomerId.Value.ToString(),
            CreatedOnUtc = aggregate.CreatedOnUtc,
            ETag = aggregate.Etag,
            Items = FromDomain(aggregate.Items),
            Metadata = new Domain.Core.Persistence.Metadata(
                aggregate.MetaData.StreamId.Value.ToString(),
                aggregate.MetaData.Version.Value,
                aggregate.MetaData.TimeStamp
            )
        };
    }

    public override ErrorOr<CartAggregate> ToDomain(Infrastructure.Persistence.Cart.Cart dto)
    {
        if (!Guid.TryParse(dto.CustomerId, out Guid customerId))
        {
            return Error.Validation("Invalid CustomerId");
        }

        if (!Guid.TryParse(dto.Id, out Guid cartId))
        {
            return Error.Validation("Invalid CustomerId");
        }

        var transformedItemsResult = _cartItemTransformer.ToDomain(dto.Items);

        if (transformedItemsResult.IsError)
        {
            return ErrorOr.ErrorOr.From(transformedItemsResult.Errors).Value;
        }
        
        return new CartAggregate(dto.CreatedOnUtc, new CustomerId(customerId))
        {
            Id = new CartId(cartId),
            Etag = dto.ETag,
            Items = transformedItemsResult.Value,
            MetaData = new MetaData(new StreamId(cartId), new Version(dto.Metadata.Version), dto.Metadata.Timestamp) 
        };
    }

    private IEnumerable<Infrastructure.Persistence.Cart.CartItem> FromDomain(IEnumerable<CartItem> items)
    {
        return items
            .Select(_cartItemTransformer.FromDomain)
            .ToList();
    }

    private ErrorOr<CartItem> ToDomain(Infrastructure.Persistence.Cart.CartItem dto)
    {
        Sku sku = new Sku(dto.Sku);
        return new CartItem(sku, dto.Quantity);
    }
}