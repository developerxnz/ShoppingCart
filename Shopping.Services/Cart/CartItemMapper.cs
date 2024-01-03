using ErrorOr;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Services.Cart;
//
// public sealed class CartItemMapper : Mapper<,,,>
// {
//     public override Infrastructure.Persistence.Cart.CartItem FromDomain(CartItem aggregate)
//     {
//         return new Infrastructure.Persistence.Cart.CartItem(aggregate.Sku.Value, aggregate.Quantity.Value);
//     }
//
//     public override ErrorOr<CartItem> ToDomain(Infrastructure.Persistence.Cart.CartItem dto)
//     {
//         Sku sku = new Sku(dto.Sku);
//         var quantityResult = CartQuantity.Create(dto.Quantity);
//         return
//             quantityResult.Match(
//                 quantity => new CartItem(sku, quantity),
//                 errors =>
//                 {
//                     ErrorOr<CartItem> errorResult = ErrorOr.ErrorOr.From(errors).Value;
//                     return errorResult;
//                 });
//     }
// }