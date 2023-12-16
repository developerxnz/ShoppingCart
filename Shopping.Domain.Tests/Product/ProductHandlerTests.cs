using ErrorOr;
using Shopping.Cart.Projections;
using Shopping.Product;
using Shopping.Product.Commands;
using Shopping.Product.Handlers;
using Shopping.Product.Core;
using Shopping.Core;

namespace ShoppingUnitTests;

public class ProductHandlerTests
{
    private readonly ICommandHandler _commandHandler;
    private readonly DateTime _now;
    private readonly CorrelationId _correlationId;
    private readonly CustomerId _customerId;
    private readonly Sku _sku;
    private readonly ProductDescription _description;
    private readonly ProductPrice _price;
    private readonly ProductId _productId;
    
    public ProductHandlerTests()
    {
        _now = DateTime.UtcNow;
        _correlationId = new (Guid.NewGuid());
        _customerId = new (Guid.NewGuid());
        _sku = new Sku(Guid.NewGuid().ToString());
        _commandHandler = new ProductCommandHandler();
        _description = new ("Product Description");
        _price = new(100m);
        _productId = new (Guid.NewGuid());
    }

    [Fact]
    public void CreateProduct_Should_Return_Successful()
    {
        CreateProductCommand command = new(_correlationId, _now, _sku, _description, _price);

        _commandHandler.HandlerForNew(command)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Core.Version(1), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(_now, result.Aggregate.MetaData.TimeStamp);
                    Assert.Equal(_now, result.Aggregate.CreatedOnUtc);
                },
                errors => Assert.Fail("Expected Product")
            );
    }
    
    [Fact]
    public void HandleNew_For_UpdateProduct_Should_Fail_When_Invalid_Command()
    {
        UpdateProductCommand command = new(_correlationId, _now, _productId, _sku, _description, _price);
        
        _commandHandler.HandlerForNew(command)
            .Switch(
                result => { Assert.Fail($"Expected {Constants.InvalidVersionDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Unexpected)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    var errorDescription = string.Format(Constants.InvalidCommandForNewDescription, typeof(UpdateProductCommand));
                    
                    Assert.Equal(Constants.InvalidCommandForNewCode, code);
                    Assert.Equal(errorDescription, description);
                }
            );
    }
}