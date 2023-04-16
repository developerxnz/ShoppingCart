using Shopping.Core;
using Shopping.Product;
using MetaData = Shopping.Cart.Persistence.MetaData;
using ErrorOr;

namespace Shopping.Cart;

public class CartTransformer : ITransformer<CartAggregate, Persistence.Cart>
{
    public Persistence.Cart FromDomain(CartAggregate domain)
    {
        return new Persistence.Cart
        {
            Id = domain.Id.Value.ToString(),
            CustomerId = domain.CustomerId.Value.ToString(),
            CreatedOnUtc = domain.CreatedOnUtc,
            ETag = domain.Etag,
            Items = FromDomain(domain.Items),
            MetaData = new MetaData(
                domain.MetaData.StreamId.Value.ToString(),
                domain.MetaData.Version.Value,
                domain.MetaData.TimeStamp
            )
        };
    }

    public ErrorOr<CartAggregate> ToDomain(Persistence.Cart dto)
    {
        if (!Guid.TryParse(dto.CustomerId, out Guid customerId))
        {
            return Error.Validation("Invalid CustomerId");
        }

        if (!Guid.TryParse(dto.Id, out Guid cartId))
        {
            return Error.Validation("Invalid CustomerId");
        }

        var transformedItemsResult =
            ToDomain(dto.Items);

        if (transformedItemsResult.IsError)
        {
            return ErrorOr.ErrorOr.From(transformedItemsResult.Errors).Value;
        }
        
        return new CartAggregate(dto.CreatedOnUtc, new CustomerId(customerId))
        {
            Id = new CartId(cartId),
            Etag = dto.ETag,
            Items = transformedItemsResult.Value,
            MetaData = new Core.MetaData(new StreamId(cartId), new(dto.MetaData.Version), dto.MetaData.TimeStamp) 
        };
    }

    public ErrorOr<IEnumerable<CartAggregate>> ToDomain(IEnumerable<Persistence.Cart> dtos)
    {
        var converted = new List<CartAggregate>();
        foreach (var dto in dtos)
        {
            var response = ToDomain(dto);
            if (response.IsError)
            {
                return response.Errors;
            }
            
            converted.Add(response.Value);
        }

        return converted;
    }

    public IEnumerable<Persistence.Cart> FromDomain(IEnumerable<CartAggregate> domains)
    {
        return domains.Select(FromDomain).ToList();
    }

    public ErrorOr<IEnumerable<CartItem>> ToDomain(IEnumerable<Persistence.CartItem> dtos)
    {
        var converted = new List<CartItem>();
        foreach (var dto in dtos)
        {
            var t = ToDomain(dto);

            if (t.IsError)
            {
                return t.Errors;
            }

            converted.Add(t.Value);
        }

        return converted;
    }
    
    private IEnumerable<Persistence.CartItem> FromDomain(IEnumerable<CartItem> items)
    {
        return items.Select(FromDomain).ToList();
    }

    private Persistence.CartItem FromDomain(CartItem domain)
    {
        return new Persistence.CartItem(domain.Sku.Value.ToString(), domain.Quantity);
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