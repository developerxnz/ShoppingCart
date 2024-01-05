using ErrorOr;
using MediatR;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Shopping.Services.Cart;

namespace Shopping.Api.Cart.Handler;

public record AddToCartRequest(CustomerId CustomerId, Sku Sku, CartQuantity Quantity, CorrelationId CorrelationId) : 
    IRequest<ErrorOr<AddToCartResponse>>;

public record AddToCartResponse(CartId CartId, CorrelationId CorrelationId);

public class AddToCartCommandHandler
    : IRequestHandler<AddToCartRequest, ErrorOr<AddToCartResponse>>
{
    private readonly ICartService _cartService;

    public AddToCartCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<ErrorOr<AddToCartResponse>> Handle(AddToCartRequest request, CancellationToken cancellationToken)
    {
        var addToCartResponse = 
            await _cartService
            .AddToCartAsync(request.CustomerId, request.Sku, request.Quantity, request.CorrelationId,
                cancellationToken);
        return
            addToCartResponse
            .Match<ErrorOr<AddToCartResponse>>(
                response => new AddToCartResponse(response.CartId, response.CorrelationId),
                errors =>
                {
                    ErrorOr<AddToCartResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return errorResult;
                }
            );
    }
}