using Shopping.Product.Core;

namespace Shopping.Cart.Core;

public record CartId(Guid Value)
{
    public static CartId Create()
    {
        return new CartId(Guid.NewGuid());
    }
};

public record CartItem(Sku Sku, uint Quantity);