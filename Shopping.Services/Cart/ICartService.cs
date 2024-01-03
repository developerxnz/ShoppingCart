using ErrorOr;
using Shopping.Domain.Cart.Requests;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Services.Cart;

public interface ICartService
{
    /// <summary>
    /// Adds an item to the Cart
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="sku"></param>
    /// <param name="quantity"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<AddToCartResponse>> AddToCartAsync(CustomerId customerId, Sku sku, CartQuantity quantity,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an item to the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<AddToCartResponse>> AddToCartAsync(AddToCartRequest request, CorrelationId correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Removes and item from the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<RemoveItemFromCartResponse>> RemoveFromCartAsync(RemoveItemFromCartRequest request,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an Item in the Cart
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<UpdateCartItemResponse>> UpdateCartAsync(UpdateCartItemRequest request, CorrelationId correlationId,
        CancellationToken cancellationToken);
}