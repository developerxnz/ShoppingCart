using ErrorOr;
using Moq;
using Shopping.Cart;
using Shopping.Cart.Commands;
using Shopping.Cart.Core;
using Shopping.Cart.Requests;
using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product;
using Shopping.Product.Core;
using Shopping.Services.Cart;
using Cart = Shopping.Infrastructure.Persistence.Cart.Cart;
using CartItem = Shopping.Infrastructure.Persistence.Cart.CartItem;
using MetaData = Shopping.Core.Persistence.Metadata;
using Version = Shopping.Core.Version;

namespace ShoppingUnitTests;

public class CartTests
{
    private readonly Shopping.Services.Cart.Cart _cart;
    private readonly Mock<IRepository<Cart>> _repository;
    private readonly Mock<ICartCommandHandler> _cartHandler;

    public CartTests()
    {
        var transformer = new CartTransformer(new CartItemTransformer());
        
        _cartHandler = new Mock<ICartCommandHandler>();
        _repository = new Mock<IRepository<Cart>>();
        _cart = new Shopping.Services.Cart.Cart(_cartHandler.Object, _repository.Object, transformer);
    }
    
    [Fact]
    public async Task AddToCart_ForNew_Should_Return_Error_When_CommandHandler_Returns_Error()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        Sku sku = new Sku(Guid.NewGuid().ToString());

        ErrorOr<CommandResult<CartAggregate>> commandResult = ErrorOr<CommandResult<CartAggregate>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForNew(It.IsAny<ICartCommand>()))
            .Returns(commandResult);

        var response = await _cart.AddToCartAsync(customerId, sku, 10, correlationId, cancellationToken);
        response.Switch(
            addToCartResponse => Assert.Fail($"Expected {nameof(AddToCartResponse)}"),
            errors =>
            {
                var (code, description) =
                    errors
                        .Where(x => x.Type == ErrorType.Validation)
                        .Select(x => (x.Code, x.Description))
                        .First();

                Assert.Equal(Constants.InvalidQuantityCode, code);
                Assert.Equal(Constants.InvalidQuantityDescription, description);
            });
    }

    [Fact]
    public async Task AddToCart_ForNew_Should_Return_AddToCartResponse_When_Valid()
    {
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);

        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        Sku sku = new Sku(Guid.NewGuid().ToString());

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);
        
        _cartHandler
            .Setup(x => x.HandlerForNew(It.IsAny<ICartCommand>()))
            .Returns(commandResult);

        var response = await _cart.AddToCartAsync(customerId, sku, 10, correlationId, cancellationToken);
        response.Switch(
            addToCartResponse =>
            {
                Assert.Equal(addToCartResponse.CorrelationId, correlationId);
                Assert.Equal(addToCartResponse.CartId, cartId);
            },
            error => Assert.Fail($"Expected {Constants.InvalidQuantityDescription}")
        );
    }

    [Fact]
    public async Task AddToCart_ForExisting_Should_Return_Error_When_CommandHandler_Returns_Error()
    {
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);
        Sku sku = new Sku(Guid.NewGuid().ToString());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            ETag = Guid.NewGuid().ToString()
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        AddToCartRequest request = new AddToCartRequest(customerId, cartId, sku, 10);

        ErrorOr<CommandResult<CartAggregate>> commandResult = ErrorOr<CommandResult<CartAggregate>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cart.AddToCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            addToCartResponse => Assert.Fail($"Expected {Constants.InvalidQuantityDescription}"),
            errors =>
            {
                var (code, description) =
                    errors
                        .Where(x => x.Type == ErrorType.Validation)
                        .Select(x => (x.Code, x.Description))
                        .First();

                Assert.Equal(Constants.InvalidQuantityCode, code);
                Assert.Equal(Constants.InvalidQuantityDescription, description);
            });
    }
    
    [Fact]
    public async Task AddToCart_ForExisting_Should_Return_AddToCartResponse_When_Valid()
    {
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);
        Sku sku = new Sku(Guid.NewGuid().ToString());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
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
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cart.AddToCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            addToCartResponse =>
            {
                Assert.Equal(addToCartResponse.CorrelationId, correlationId);
                Assert.Equal(addToCartResponse.CartId, cartId);
            },
            error => Assert.Fail($"Expected {(nameof(AddToCartResponse))}")
        );
    }

    [Fact]
    public async Task UpdateCartItem_ForExisting_Should_Return_Error_When_CommandHandler_Returns_Error()
    {
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);
        Sku sku = new Sku(Guid.NewGuid().ToString());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            ETag = Guid.NewGuid().ToString()
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, 10);

        ErrorOr<CommandResult<CartAggregate>> commandResult = ErrorOr<CommandResult<CartAggregate>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cart.UpdateCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            updateCartItmResponse => Assert.Fail($"Expected {Constants.InvalidQuantityDescription}"),
            errors =>
            {
                var (code, description) =
                    errors
                        .Where(x => x.Type == ErrorType.Validation)
                        .Select(x => (x.Code, x.Description))
                        .First();

                Assert.Equal(Constants.InvalidQuantityCode, code);
                Assert.Equal(Constants.InvalidQuantityDescription, description);
            });
    }
    
    [Fact]
    public async Task UpdateCartItem_ForExisting_Should_Return_UpdateCartItemResponse_When_Valid()
    {
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);
        Sku sku = new Sku(Guid.NewGuid().ToString());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
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

        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cart.UpdateCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            updateCartItemResponse =>
            {
                Assert.Equal(typeof(UpdateCartItemResponse), updateCartItemResponse.GetType());
                Assert.Equal(updateCartItemResponse.CorrelationId, correlationId);
                Assert.Equal(updateCartItemResponse.CartId, cartId);
            },
            error => { Assert.Fail($"Expected {nameof(AddToCartResponse)}"); }
        );
    }

    [Fact]
    public async Task RemoveItemFromCart_ForExisting_Should_Return_AddToCartResponse_When_Valid()
    {
        DateTime timeStamp = DateTime.UtcNow;
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);
        Sku sku = new Sku(Guid.NewGuid().ToString());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
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
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cart.UpdateCartAsync(request, correlationId, cancellationToken);
        response.Switch(
            removeItemFromCartResponse =>
            {
                Assert.Equal(typeof(UpdateCartItemResponse), removeItemFromCartResponse.GetType());
                Assert.Equal(removeItemFromCartResponse.CorrelationId, correlationId);
                Assert.Equal(removeItemFromCartResponse.CartId, cartId);
            },
            error => Assert.Fail($"Expected {nameof(UpdateCartItemResponse)}")
        );
    }
    
    [Fact]
    public async Task RemoveItemFromCart_ForExisting_Should_Return_Error_When_CommandHandler_Returns_Error()
    {
        CartId cartId = new CartId(Guid.NewGuid());
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        StreamId streamId = new StreamId(Guid.NewGuid());
        Version version = new Version(10);
        Sku sku = new Sku(Guid.NewGuid().ToString());
        Cart dto = new Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            ETag = Guid.NewGuid().ToString()
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, 30);

        ErrorOr<CommandResult<CartAggregate>> commandResult = ErrorOr<CommandResult<CartAggregate>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cart.UpdateCartAsync(request, correlationId, cancellationToken);
        response.Switch(
                updateCartItmResponse => Assert.Fail($"Expected {Constants.InvalidQuantityDescription}"),
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.InvalidQuantityCode, code);
                    Assert.Equal(Constants.InvalidQuantityDescription, description);
                });
    }
}