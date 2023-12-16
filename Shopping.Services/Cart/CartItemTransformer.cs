using ErrorOr;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Services.Cart;

public sealed class CartItemTransformer : Transformer<CartItem, Infrastructure.Persistence.Cart.CartItem>
{
    public override Infrastructure.Persistence.Cart.CartItem FromDomain(CartItem aggregate)
    {
        return new Infrastructure.Persistence.Cart.CartItem(aggregate.Sku.Value, aggregate.Quantity);
    }

    public override ErrorOr<CartItem> ToDomain(Infrastructure.Persistence.Cart.CartItem dto)
    {
        Sku sku = new Sku(dto.Sku);
        
        return new CartItem(sku, dto.Quantity);
    }
}