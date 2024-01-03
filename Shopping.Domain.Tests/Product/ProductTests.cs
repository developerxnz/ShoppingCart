using Moq;
using Shopping.Domain.Core;
using Shopping.Domain.Product.Commands;
using Shopping.Domain.Product.Core;
using Shopping.Infrastructure.Interfaces;

namespace ShoppingUnitTests;

public class ProductTests
{
    private readonly Mock<IRepository<Shopping.Infrastructure.Persistence.Products.Product>> _repository;
    private readonly Shopping.Domain.Product.Handlers.ICommandHandler _handler;

    public ProductTests()
    {
        _handler = new Shopping.Domain.Product.Handlers.ProductCommandHandler();
        _repository = new Mock<IRepository<Shopping.Infrastructure.Persistence.Products.Product>>();
    }

    [Fact]
    public void AddToCart_ForNew_Should_Return_Error_When_CommandHandler_Returns_Error()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        Sku sku = new Sku(Guid.NewGuid().ToString());
        DateTime createdOnUtc = DateTime.UtcNow;
        ProductDescription description = new ProductDescription("Descritiption");
        ProductPrice price = new ProductPrice(100);

        CreateProductCommand command = new CreateProductCommand(correlationId, createdOnUtc, sku, description, price);

        var commandResult = _handler.HandlerForNew(command);
        
        commandResult.Switch(
            value =>
            {
                var (aggregate, events) = value;
                Assert.Equal(createdOnUtc, aggregate.CreatedOnUtc);
                Assert.Equal(description, aggregate.Description);
                Assert.Equal(price, aggregate.Price);
                Assert.Equal(sku, aggregate.Sku);
                Assert.Equal(1u, aggregate.MetaData.Version.Value);
            },
            errors =>
            { 
                Assert.Fail("Expected Aggregate and Events");
            });
    }
}