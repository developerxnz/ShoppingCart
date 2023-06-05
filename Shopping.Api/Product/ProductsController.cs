using Microsoft.AspNetCore.Mvc;
using Shopping.Core;
using Shopping.Product.Core;
using Shopping.Product.Services;

namespace Shopping.Api.Controllers;

[ApiController]
public class ProductsController : ControllerBase
{
    private ILogger _logger;
    private readonly IProduct _product;

    public ProductsController(IProduct product, ILogger<ProductsController> logger)
    {
        _product = product;
        _logger = logger;
    }

    [Route("/[controller]/")]
    [HttpPost]
    public async Task<IActionResult> Post(CreateProductRequest request, CancellationToken cancellationToken)
    {
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Product.CreateProductRequest serviceRequest =
            new(
                new Sku(request.Sku),
                new(request.Description),
                new(request.Cost)
            );

        var x = await _product.Create(correlationId, cancellationToken, serviceRequest);

        return x
            .Match<IActionResult>(
            onValue => new OkObjectResult(onValue),
            onError => new BadRequestResult());
    }
    
    [Route("/[controller]/{productId:guid}")]
    [HttpPut]
    public async Task<IActionResult> Put(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Product.UpdateProductRequest serviceRequest =
            new(
                new ProductId(productId),
                new Sku(request.Sku),
                new(request.Description),
                new(request.Cost)
            );

        var x = await _product.Update(correlationId, cancellationToken, serviceRequest);

        return x
            .Match<IActionResult>(
                onValue => new OkObjectResult(onValue),
                onError => new BadRequestResult());
    }
}

public class CreateProductRequest
{
    public string Description { get; set; }

    public decimal Cost { get; set; }

    public string Sku { get; set; }
}

public class UpdateProductRequest
{
    public string Description { get; set; }

    public decimal Cost { get; set; }

    public string Sku { get; set; }
}