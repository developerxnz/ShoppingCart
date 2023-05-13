using Shopping.Core;
using Shopping.Product;
using ErrorOr;
using Shopping.Cart.Core;
using Version = Shopping.Core.Version;

namespace Shopping.Cart;

public sealed class CartTransformer : Transformer<CartAggregate, Persistence.Cart>
{
    private readonly CartItemTransformer _cartItemTransformer;

    public CartTransformer(CartItemTransformer cartItemTransformer)
    {
        _cartItemTransformer = cartItemTransformer;
    }

    public override Persistence.Cart FromDomain(CartAggregate domain)
    {
        return new Persistence.Cart
        {
            Id = domain.Id.Value.ToString(),
            CustomerId = domain.CustomerId.Value.ToString(),
            CreatedOnUtc = domain.CreatedOnUtc,
            ETag = domain.Etag,
            Items = FromDomain(domain.Items),
            MetaData = new Shopping.Core.Persistence.MetaData(
                domain.MetaData.StreamId.Value.ToString(),
                domain.MetaData.Version.Value,
                domain.MetaData.TimeStamp
            )
        };
    }

    public override ErrorOr<CartAggregate> ToDomain(Persistence.Cart dto)
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
            MetaData = new MetaData(new StreamId(cartId), new Version(dto.MetaData.Version), dto.MetaData.TimeStamp) 
        };
    }

    private IEnumerable<Persistence.CartItem> FromDomain(IEnumerable<CartItem> items)
    {
        return items
            .Select(_cartItemTransformer.FromDomain)
            .ToList();
    }

    private ErrorOr<CartItem> ToDomain(Persistence.CartItem dto)
    {
        if (!Guid.TryParse(dto.Sku, out Guid skuGuid))
        {
            return Error.Validation("Invalid Sku");
        }

        Sku sku = new Sku(skuGuid);
        return new CartItem(sku, dto.Quantity);
    }
}

public sealed class CartItemTransformer : Transformer<CartItem, Persistence.CartItem>
{
    public override Persistence.CartItem FromDomain(CartItem domain)
    {
        return new Persistence.CartItem(domain.Sku.Value.ToString(), domain.Quantity);
    }

    public override ErrorOr<CartItem> ToDomain(Persistence.CartItem dto)
    {
        if (!Guid.TryParse(dto.Sku, out Guid skuGuid))
        {
            return Error.Validation($"Invalid {nameof(dto.Sku)}");
        }

        Sku sku = new Sku(skuGuid);
        
        return new CartItem(sku, dto.Quantity);
    }
}