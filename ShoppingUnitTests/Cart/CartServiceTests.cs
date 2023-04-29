using ErrorOr;
using Moq;
using Shopping.Cart;
using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product;
using Cart = Shopping.Cart.Persistence.Cart;
using CartItem = Shopping.Cart.Persistence.CartItem;
using MetaData = Shopping.Cart.Persistence.MetaData;
using Version = Shopping.Core.Version;

namespace ShoppingUnitTests;

public class CartTests
{
    [Fact]
    public async Task AddToCart_ForNew_Should_Return_AddToCartResponse_When_Valid()
    {
        CartTransformer transformer = new CartTransformer();
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);
        
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            MetaData = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = Enumerable.Empty<CartItem>(),
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
        ErrorOr<CommandResult<CartAggregate>> commandResult2 = ErrorOr<CommandResult<CartAggregate>>.From(new List<Error> { Error.Failure("", "") });

        Mock<ICartCommandHandler> cartHandler = new Mock<ICartCommandHandler>();
        cartHandler
            .Setup(x => x.HandlerForNew(It.IsAny<ICartCommand>()))
            .Returns(commandResult);

        Mock<IRepository<Cart>> repository = new Mock<IRepository<Cart>>();
        Shopping.Cart.Cart cart = new Shopping.Cart.Cart(cartHandler.Object, repository.Object, transformer);

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
        
        Version version = new Version(10);
        
        Sku sku = new Sku(Guid.NewGuid());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            MetaData = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new []{ new CartItem(sku.Value.ToString(), 10 ) },
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

        Mock<IRepository<Cart>> repository = new Mock<IRepository<Cart>>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        
        Shopping.Cart.Cart cart = new Shopping.Cart.Cart(cartHandler.Object, repository.Object, transformer);

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
    
    [Fact]
    public async Task UpdateCartItem_ForExisting_Should_Return_UpdateCartItemResponse_When_Valid()
    {
        CartTransformer transformer = new CartTransformer();
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        
        Version version = new Version(10);
        
        Sku sku = new Sku(Guid.NewGuid());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            MetaData = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new []{ new CartItem(sku.Value.ToString(), 10 ) },
            ETag = Guid.NewGuid().ToString()
        };
        
        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Core.MetaData(streamId, version, timeStamp)
        };
        
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, 10);

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);

        Mock<ICartCommandHandler> cartHandler = new Mock<ICartCommandHandler>();
        cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);

        Mock<IRepository<Cart>> repository = new Mock<IRepository<Cart>>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        
        Shopping.Cart.Cart cart = new Shopping.Cart.Cart(cartHandler.Object, repository.Object, transformer);

        var response = await cart.UpdateCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            updateCartItemResponse =>
            {
                Assert.Equal(typeof(UpdateCartItemResponse), updateCartItemResponse.GetType());
                Assert.Equal(updateCartItemResponse.CorrelationId, correlationId);
                Assert.Equal(updateCartItemResponse.CartId, cartId);
            },
            error => Assert.Fail("Expected AddToCartResponse")
        );
    }
    
     [Fact]
    public async Task RemoveItemFromCart_ForExisting_Should_Return_AddToCartResponse_When_Valid()
    {
        CartTransformer transformer = new CartTransformer();
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        
        Version version = new Version(10);
        
        Sku sku = new Sku(Guid.NewGuid());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            MetaData = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new []{ new CartItem(sku.Value.ToString(), 10 ) },
            ETag = Guid.NewGuid().ToString()
        };
        
        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Core.MetaData(streamId, version, timeStamp)
        };
        
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, 30);

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);

        Mock<ICartCommandHandler> cartHandler = new Mock<ICartCommandHandler>();
        cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);

        Mock<IRepository<Cart>> repository = new Mock<IRepository<Cart>>();
        repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        
        Shopping.Cart.Cart cart = new Shopping.Cart.Cart(cartHandler.Object, repository.Object, transformer);

        var response = await cart.UpdateCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            removeItemFromCartResponse =>
            {
                Assert.Equal(typeof(UpdateCartItemResponse), removeItemFromCartResponse.GetType());
                Assert.Equal(removeItemFromCartResponse.CorrelationId, correlationId);
                Assert.Equal(removeItemFromCartResponse.CartId, cartId);
            },
            error => Assert.Fail("Expected AddToCartResponse")
        );
    }
    
}