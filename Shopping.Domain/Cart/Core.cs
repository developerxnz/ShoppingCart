using Shopping.Domain.Product.Core;

namespace Shopping.Domain.Cart.Core;

public record CartId(Guid Value)
{
    public static CartId Create()
    {
        return new CartId(Guid.NewGuid());
    }
};

public record CartItem(Sku Sku, CartQuantity Quantity);