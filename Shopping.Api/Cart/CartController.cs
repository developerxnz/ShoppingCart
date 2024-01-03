using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shopping.Domain.Cart.Core;
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
        CartQuantity quantity = new CartQuantity(request.Quantity);
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

    [Route("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Sku sku = new Sku(id);
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        
        var deleteFromCartRequest = new Shopping.Api.Cart.DeleteFromCartRequest(customerId, cartId, sku, correlationId);
        var handlerResult = await _mediator.Send(deleteFromCartRequest, cancellationToken);
        
        return
            handlerResult
                .Match<IActionResult>(
                    value => new OkObjectResult(value),
                    errors => new BadRequestObjectResult(errors));
    }
}

public record AddToCartRequest(Guid CustomerId, string Sku, uint Quantity);