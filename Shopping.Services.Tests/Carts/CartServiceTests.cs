using ErrorOr;
using Moq;
using Shopping.Domain.Cart;
using Shopping.Domain.Cart.Commands;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Cart.Events;
using Shopping.Domain.Cart.Requests;
using Shopping.Domain.Core;
using Shopping.Domain.Domain.Core.Handlers;
using Shopping.Domain.Product.Core;
using Shopping.Infrastructure.Interfaces;
using Shopping.Services.Cart;
using Cart = Shopping.Domain.Cart.Cart;
using CartItem = Shopping.Infrastructure.Persistence.Cart.CartItem;
using MetaData = Shopping.Domain.Core.Persistence.Metadata;
using Version = Shopping.Domain.Core.Version;

namespace ShoppingUnitTests;

public class CartTests
{
    private readonly Shopping.Services.Cart.Cart _cart;
    private readonly Mock<IRepository<Shopping.Infrastructure.Persistence.Cart.Cart>> _repository;
    private readonly Mock<ICartCommandHandler> _cartHandler;

    public CartTests()
    {
        var transformer = new CartMapper();
        
        _cartHandler = new Mock<ICartCommandHandler>();
        _repository = new Mock<IRepository<Shopping.Infrastructure.Persistence.Cart.Cart>>();
        _cart = new Shopping.Services.Cart.Cart(_cartHandler.Object, _repository.Object, transformer);
    }
    
    [Fact]
    public async Task AddToCart_ForNew_Should_Return_Error_When_CommandHandler_Returns_Error()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        Sku sku = new Sku(Guid.NewGuid().ToString());

        ErrorOr<CommandResult<Cart, CartEvent>> commandResult = ErrorOr<CommandResult<Cart, CartEvent>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForNew(It.IsAny<ICartCommand>()))
            .Returns(commandResult);

        var quantity = new CartQuantity(10);
        var response = await _cart.AddToCartAsync(customerId, sku, quantity, correlationId, cancellationToken);
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

        Cart aggregate = new Cart(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        Sku sku = new Sku(Guid.NewGuid().ToString());

        IEnumerable<CartEvent> events = Enumerable.Empty<CartEvent>();
        var commandResult = new CommandResult<Cart, CartEvent>(aggregate, events);
        
        _cartHandler
            .Setup(x => x.HandlerForNew(It.IsAny<ICartCommand>()))
            .Returns(commandResult);

        var quantity = new CartQuantity(10);
        var response = await _cart.AddToCartAsync(customerId, sku, quantity, correlationId, cancellationToken);
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
        Shopping.Infrastructure.Persistence.Cart.Cart dto = new Shopping.Infrastructure.Persistence.Cart.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        var quantity = new CartQuantity(10);
        AddToCartRequest request = new AddToCartRequest(customerId, cartId, sku, quantity);

        ErrorOr<CommandResult<Cart, CartEvent>> commandResult = ErrorOr<CommandResult<Cart, CartEvent>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<Cart>()))
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
        Shopping.Infrastructure.Persistence.Cart.Cart dto = new Shopping.Infrastructure.Persistence.Cart.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        Cart aggregate = new Cart(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        var quantity = new CartQuantity(10);
        AddToCartRequest request = new AddToCartRequest(customerId, cartId, sku, quantity);

        IEnumerable<CartEvent> events = Enumerable.Empty<CartEvent>();
        var commandResult = new CommandResult<Cart, CartEvent>(aggregate, events);
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<Cart>()))
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
        Shopping.Infrastructure.Persistence.Cart.Cart dto = new Shopping.Infrastructure.Persistence.Cart.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        var quantity = new CartQuantity(10);
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, quantity);

        ErrorOr<CommandResult<Cart, CartEvent>> commandResult = ErrorOr<CommandResult<Cart, CartEvent>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<Cart>()))
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
        Shopping.Infrastructure.Persistence.Cart.Cart dto = new Shopping.Infrastructure.Persistence.Cart.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        Cart aggregate = new Cart(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        var quantity = new CartQuantity(10);
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, quantity);

        IEnumerable<CartEvent> events = Enumerable.Empty<CartEvent>();
        var commandResult = new CommandResult<Cart, CartEvent>(aggregate, events);

        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<Cart>()))
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
        Shopping.Infrastructure.Persistence.Cart.Cart dto = new Shopping.Infrastructure.Persistence.Cart.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        Cart aggregate = new Cart(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        var quantity = new CartQuantity(30);
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, quantity);

        IEnumerable<CartEvent> events = Enumerable.Empty<CartEvent>();
        var commandResult = new CommandResult<Cart, CartEvent>(aggregate, events);
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<Cart>()))
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
        Shopping.Infrastructure.Persistence.Cart.Cart dto = new Shopping.Infrastructure.Persistence.Cart.Cart
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        var quantity = new CartQuantity(30);
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, quantity);

        ErrorOr<CommandResult<Cart,CartEvent>> commandResult = ErrorOr<CommandResult<Cart, CartEvent>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<Cart>()))
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