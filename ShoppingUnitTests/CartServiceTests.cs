using Moq;
using Shopping.Cart;
using Shopping.Cart.Persistence;
using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product;
using Cart = Shopping.Cart.Serivces.Cart;
using Version = System.Version;

namespace ShoppingUnitTests;

public class CartServiceTests
{
    public CartServiceTests()
    {
    }

    [Fact]
    public async Task AddToCart_ForNew_Should_Return_AddToCartResponse_When_Valid()
    {
        CartTransformer transformer = new CartTransformer();
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        
        Shopping.Core.Version version = new Shopping.Core.Version(10);
        
        Shopping.Cart.Persistence.Cart dto = new Shopping.Cart.Persistence.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            MetaData = new Shopping.Cart.Persistence.MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = Enumerable.Empty<Shopping.Cart.Persistence.CartItem>(),
            ETag = Guid.NewGuid().ToString()
        };
        
        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Core.MetaData(streamId, version, timeStamp)
        };
        
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        Sku sku = new Sku(Guid.NewGuid());
        
        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);

        Mock<ICartCommandHandler> cartHandler = new Mock<ICartCommandHandler>();
        cartHandler
            .Setup(x => x.HandlerForNew(It.IsAny<ICartCommand>()))
            .Returns(commandResult);

        Mock<IRepository<Shopping.Cart.Persistence.Cart>> repository = new Mock<IRepository<Shopping.Cart.Persistence.Cart>>();
        Cart cart = new Shopping.Cart.Serivces.Cart(cartHandler.Object, repository.Object, transformer);

        var response = await cart.AddToCartAsync(customerId, sku, 10, correlationId, cancellationToken);
        response.Switch(
            addToCartResponse =>
            {
                Assert.Equal(addToCartResponse.CorrelationId, correlationId);
                Assert.Equal(addToCartResponse.CartId, cartId);
            },
            error => Assert.Fail("Expected AddToCartResponse")
        );
    }
    
    [Fact]
    public async Task AddToCart_ForExisting_Should_Return_AddToCartResponse_When_Valid()
    {
        CartTransformer transformer = new CartTransformer();
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        
        Shopping.Core.Version version = new Shopping.Core.Version(10);
        
        Sku sku = new Sku(Guid.NewGuid());
        Shopping.Cart.Persistence.Cart dto = new Shopping.Cart.Persistence.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            MetaData = new Shopping.Cart.Persistence.MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new []{ new Shopping.Cart.Persistence.CartItem(sku.Value.ToString(), 10 ) },
            ETag = Guid.NewGuid().ToString()
        };
        
        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Core.MetaData(streamId, version, timeStamp)
        };
        
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        AddToCartRequest request = new AddToCartRequest(customerId, cartId, sku, 10);

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);

        Mock<ICartCommandHandler> cartHandler = new Mock<ICartCommandHandler>();
        cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);

        Mock<IRepository<Shopping.Cart.Persistence.Cart>> repository = new Mock<IRepository<Shopping.Cart.Persistence.Cart>>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        
        Cart cart = new Shopping.Cart.Serivces.Cart(cartHandler.Object, repository.Object, transformer);

        var response = await cart.AddToCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            addToCartResponse =>
            {
                Assert.Equal(addToCartResponse.CorrelationId, correlationId);
                Assert.Equal(addToCartResponse.CartId, cartId);
            },
            error => Assert.Fail("Expected AddToCartResponse")
        );
    }
}