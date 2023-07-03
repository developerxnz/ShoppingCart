using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Shopping.Core;
using Shopping.Product.Core;
using Shopping.Product.Services;
using ValidationResult = FluentValidation.Results.ValidationResult;
using ErrorOr;

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
        CreateProductRequestValidator validator = new CreateProductRequestValidator();
        CorrelationId correlationId = new(Guid.NewGuid());

        ValidationResult? validationResponse = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResponse.IsValid)
        {
            return new BadRequestResult();
        }
            
        Shopping.Product.CreateProductRequest serviceRequest =
            new(
                new Sku(request.Sku),
                new(request.Description),
                new(request.Cost)
            );

        ErrorOr<Product.CreateProductResponse> x = await _product.Create(correlationId, cancellationToken, serviceRequest);
                
        return x.Match<IActionResult>(
        onValue =>
        {
            var dto = new CreateProductResponse(onValue.ProductId, onValue.CorrelationId);
            return new OkObjectResult(dto);
        },
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
    [Required]
    public string Description { get; set; }

    [Required]
    public decimal Cost { get; set; }

    [Required]
    public string Sku { get; set; }
}

public class CreateProductResponse
{
    public string CorrelationId { get; private set; }
    public string ProductId { get; private set; }

    public CreateProductResponse(ProductId productId, CorrelationId correlationId)
    {
        CorrelationId = correlationId.ToString();
        ProductId = productId.Value.ToString();
    }
    
};

public class UpdateProductRequest
{
    [Required]
    public string Description { get; set; }

    [Required]
    public decimal Cost { get; set; }

    [Required]
    public string Sku { get; set; }
}