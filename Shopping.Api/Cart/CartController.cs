using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Api.Cart;

[ApiController]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        var quantityResult = CartQuantity.Create(request.Quantity);
        if (quantityResult.IsError)
        {
            return new BadRequestResult();
        }

        CartQuantity quantity = quantityResult.Value;
        Sku sku = new Sku(request.Sku);
        CustomerId customerId = new CustomerId(request.CustomerId);
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        
        var addToCartRequest = new Shopping.Api.Cart.Handler.AddToCartRequest(customerId, sku, quantity, correlationId);
        var handlerResult = await _mediator.Send(addToCartRequest, cancellationToken);
        return
            handlerResult
                .Match<IActionResult>(
                    value => new OkObjectResult(value),
                    errors => new BadRequestObjectResult(errors));
        }
}

public record AddToCartRequest(Guid CustomerId, string Sku, uint Quantity);