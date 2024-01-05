using MediatR;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using ErrorOr;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Cart.Requests;
using Shopping.Services.Cart;

namespace Shopping.Api.Cart;

public sealed record DeleteFromCartRequest(CustomerId CustomerId, CartId CartId, Sku Sku, CorrelationId CorrelationId) : IRequest<ErrorOr<DeleteFromCartResponse>>;
public sealed record DeleteFromCartResponse(CorrelationId CorrelationId);

public class DeleteFromCartCommandHandler : IRequestHandler<DeleteFromCartRequest, ErrorOr<DeleteFromCartResponse>>
{
    private readonly ICartService _cartService;

    public DeleteFromCartCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<ErrorOr<DeleteFromCartResponse>> Handle(DeleteFromCartRequest request, CancellationToken cancellationToken)
    {
        var x = new RemoveItemFromCartRequest(request.CustomerId, request.CartId, request.Sku);
        var deleteFromCartResponse = await _cartService.RemoveFromCartAsync(x, request.CorrelationId, cancellationToken);

        return deleteFromCartResponse
            .Match<ErrorOr<DeleteFromCartResponse>>(
                value => new DeleteFromCartResponse(value.CorrelationId),
                errors =>
                {
                    ErrorOr<DeleteFromCartResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return errorResult;
                });
    }
}