using ErrorOr;
using Moq;
using Shopping.Domain.Cart;
using Shopping.Domain.Cart.Commands;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Cart.Requests;
using Shopping.Domain.Core;
using Shopping.Domain.Domain.Core.Handlers;
using Shopping.Domain.Product.Core;
using Shopping.Infrastructure.Interfaces;
using Shopping.Services.Cart;
using CartAggregate = Shopping.Domain.Cart.CartAggregate;
using CartItem = Shopping.Infrastructure.Persistence.Cart.CartItem;
using MetaData = Shopping.Domain.Core.Persistence.Metadata;
using Version = Shopping.Domain.Core.Version;

namespace ShoppingUnitTests;

public class ProductTests
{
    private readonly Shopping.Services.Cart.CartAggregate _cartAggregate;
    private readonly Mock<IRepository<Shopping.Infrastructure.Persistence.Cart.CartAggregate>> _repository;
    private readonly Mock<ICartCommandHandler> _cartHandler;

    public ProductTests()
    {
        var transformer = new CartMapper(new Shopping.Services.Cart.CartItemMapper());
        
        _cartHandler = new Mock<ICartCommandHandler>();
        _repository = new Mock<IRepository<Shopping.Infrastructure.Persistence.Cart.CartAggregate>>();
        _cartAggregate = new Shopping.Services.Cart.CartAggregate(_cartHandler.Object, _repository.Object, transformer);
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

        var response = await _cartAggregate.AddToCartAsync(customerId, sku, CartQuantity.Create(10).Value, correlationId, cancellationToken);
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
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        Sku sku = new Sku(Guid.NewGuid().ToString());

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);
        
        _cartHandler
            .Setup(x => x.HandlerForNew(It.IsAny<ICartCommand>()))
            .Returns(commandResult);

        var response = await _cartAggregate.AddToCartAsync(customerId, sku, CartQuantity.Create(10).Value, correlationId, cancellationToken);
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
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new Shopping.Infrastructure.Persistence.Cart.CartAggregate
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value, 10)},
            Etag = Guid.NewGuid().ToString()
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        AddToCartRequest request = new AddToCartRequest(customerId, cartId, sku, CartQuantity.Create(10).Value);

        ErrorOr<CommandResult<CartAggregate>> commandResult = ErrorOr<CommandResult<CartAggregate>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cartAggregate.AddToCartAsync(request, correlationId, cancellationToken);
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
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new Shopping.Infrastructure.Persistence.Cart.CartAggregate
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        AddToCartRequest request = new AddToCartRequest(customerId, cartId, sku, CartQuantity.Create(10).Value);

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cartAggregate.AddToCartAsync(request, correlationId, cancellationToken);
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
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new Shopping.Infrastructure.Persistence.Cart.CartAggregate
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
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, CartQuantity.Create(10).Value);

        ErrorOr<CommandResult<CartAggregate>> commandResult = ErrorOr<CommandResult<CartAggregate>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cartAggregate.UpdateCartAsync(request, correlationId, cancellationToken);
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
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new Shopping.Infrastructure.Persistence.Cart.CartAggregate
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, CartQuantity.Create(10).Value);

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);

        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cartAggregate.UpdateCartAsync(request, correlationId, cancellationToken);
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
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new Shopping.Infrastructure.Persistence.Cart.CartAggregate
        {
            Id = cartId.Value.ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            CreatedOnUtc = default,
            Metadata = new MetaData(streamId.Value.ToString(), version.Value, DateTime.UtcNow),
            Items = new[] {new CartItem(sku.Value.ToString(), 10)},
            Etag = Guid.NewGuid().ToString()
        };

        CartAggregate aggregate = new CartAggregate(DateTime.UtcNow, customerId)
        {
            Id = cartId,
            MetaData = new Shopping.Domain.Core.MetaData(streamId, version, timeStamp)
        };

        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        CancellationToken cancellationToken = new CancellationToken();
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, CartQuantity.Create(30).Value);

        IEnumerable<Event> events = Enumerable.Empty<Event>();
        var commandResult = new CommandResult<CartAggregate>(aggregate, events);
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cartAggregate.UpdateCartAsync(request, correlationId, cancellationToken);
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
        Shopping.Infrastructure.Persistence.Cart.CartAggregate dto = new Shopping.Infrastructure.Persistence.Cart.CartAggregate
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
        UpdateCartItemRequest request = new UpdateCartItemRequest(customerId, cartId, sku, CartQuantity.Create(30).Value);

        ErrorOr<CommandResult<CartAggregate>> commandResult = ErrorOr<CommandResult<CartAggregate>>
            .From(new List<Error>
                {Error.Validation(Constants.InvalidQuantityCode, Constants.InvalidQuantityDescription)});
        
        _cartHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<ICartCommand>(), It.IsAny<CartAggregate>()))
            .Returns(commandResult);
        
        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var response = await _cartAggregate.UpdateCartAsync(request, correlationId, cancellationToken);
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