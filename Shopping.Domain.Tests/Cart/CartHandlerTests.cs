using ErrorOr;
using Shopping.Domain.Cart;
using Shopping.Domain.Cart.Commands;
using Shopping.Domain.Cart.Core;
using Shopping.Domain.Cart.Events;
using Shopping.Domain.Core;
using Shopping.Domain.Domain.Core.Handlers;
using Shopping.Domain.Product.Core;
using MetaData = Shopping.Domain.Core.MetaData;

namespace ShoppingUnitTests;

public class CartHandlerTests
{
    private readonly ICartCommandHandler _commandHandler;
    private readonly DateTime _now;
    private readonly CorrelationId _correlationId;
    private readonly CustomerId _customerId;
    private readonly CartId _cartId;
    private readonly CartId _mismatchedCartId;
    private readonly Sku _sku;
    
    public CartHandlerTests()
    {
        _now = DateTime.UtcNow;
        _correlationId = new CorrelationId(Guid.NewGuid());
        _customerId = new CustomerId(Guid.NewGuid());
        _cartId = new CartId(Guid.NewGuid());
        _mismatchedCartId = new(Guid.NewGuid());
        _sku = new Sku(Guid.NewGuid().ToString());
        _commandHandler = new CartCommandHandler();
    }

    [Fact]
    public void AddItemToCart_New_Cart_Should_Return_Successful()
    {
        var quantity = new CartQuantity(10);
        AddItemToCartCommand command = new(_now, _customerId, null, _sku, quantity, _correlationId);

        _commandHandler.HandlerForNew(command)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Domain.Core.Version(1), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(_now, result.Aggregate.MetaData.TimeStamp);
                    Assert.Single(result.Aggregate.Items);
                    Assert.Equal(_now, result.Aggregate.CreatedOnUtc);
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void AddItemToCart_Existing_Cart_Should_Fail_When_Version_Is_0()
    {
        Cart aggregate = new Cart(_now, _customerId);
        var quantity = new CartQuantity(10);
        AddItemToCartCommand command = new(_now, _customerId, aggregate.Id, _sku, quantity, _correlationId);
        
        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result => { Assert.Fail($"Expected {Constants.InvalidVersionDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.InconsistentVersionCode, code);
                    Assert.Equal(Constants.InconsistentVersionDescription, description);
                }
            );
    }

    [Fact]
    public void AddItemToCart_Existing_Cart_Should_Fail_When_AggregateCheck_Fails()
    {
        var quantity = new CartQuantity(10);
        AddItemToCartCommand command = new AddItemToCartCommand(_now, _customerId, _mismatchedCartId, _sku, quantity, _correlationId);
        Cart aggregate = new Cart(_now, _customerId);
        var versionedAggregate = aggregate with { MetaData = aggregate.MetaData with { Version = new Shopping.Domain.Core.Version(2) } };

        _commandHandler.HandlerForExisting(command, versionedAggregate)
            .Switch(
                result => { Assert.Fail($"Expected: {Constants.InvalidAggregateForIdDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();
                    
                    Assert.Equal(Constants.InvalidAggregateForIdDescription, description);
                }
            );
    }
    
    [Fact]
    public void AddItemToCart_Existing_Cart_Should_Return_Successful_when_existing_aggregate()
    {
        var quantity = new CartQuantity(10);
        AddItemToCartCommand command = new(_now, _customerId, null, _sku, quantity, _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Domain.Core.Version(7), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(_now, result.Aggregate.MetaData.TimeStamp);
                    Assert.True(result.Aggregate.Items.Count() == 2);
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void AddItemToCart_Existing_Cart_Should_Fail_when_aggregate_check_fails()
    {
        var quantity = new CartQuantity(10);
        AddItemToCartCommand command = new(_now, _customerId, _cartId, _sku, quantity, _correlationId);
        
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _mismatchedCartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                _ => { Assert.Fail($"Expected: {Constants.InvalidAggregateForIdDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();
                    
                    Assert.Equal(Constants.InvalidAggregateForIdDescription, description);
                }
            );
    }

    [Fact]
    public void RemoveItemFromCart_Should_Return_Successful_When_Item_Exists()
    {
        RemoveItemFromCartCommand command = new(_now, _customerId, _cartId, _sku, _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Domain.Core.Version(7), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(_now, result.Aggregate.MetaData.TimeStamp);
                    Assert.True(!result.Aggregate.Items.Any());
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void RemoveItemFromCart_Should_Fail_when_aggregate_check_fails()
    {
        RemoveItemFromCartCommand command = new(_now, _customerId, _mismatchedCartId, _sku, _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result => { Assert.Fail($"Expected: {Constants.InvalidAggregateForIdDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();
                    
                    Assert.Equal(Constants.InvalidAggregateForIdDescription, description);
                }
            );
    }
    
    [Fact]
    public void RemoveItemFromCart_Should_Throw_Exception_Whe_Sku_Doesnt_Exist()
    {
        Sku invalidSku = new(Guid.NewGuid().ToString());
        RemoveItemFromCartCommand command = new(_now, _customerId, _cartId, invalidSku, _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _cartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<Cart, CartEvent>> result = _commandHandler.HandlerForExisting(command, aggregate);

        result
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.InvalidCartItemSkuDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.InvalidCartItemSkuCode, code);
                    Assert.Equal(Constants.InvalidCartItemSkuDescription, description);
                }
            );
    }

    [Fact]
    public void RemoveItemFromCart_Should_Throw_Exception_When_Aggregate_Check_Fails()
    {
        Sku invalidSku = new(Guid.NewGuid().ToString());
        RemoveItemFromCartCommand command = new(_now, _customerId, _cartId, invalidSku, _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _mismatchedCartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<Cart, CartEvent>> result = _commandHandler.HandlerForExisting(command, aggregate);

        result
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.InvalidAggregateForIdDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.InvalidAggregateForIdCode, code);
                    Assert.Equal(Constants.InvalidAggregateForIdDescription, description);
                }
            );
    }

    [Fact]
    public void UpdateItemInCart_ShouldThrow_Exception_When_Aggregate_Check_Fails()
    {
        UpdateItemInCartCommand command = new(_now, _customerId, _mismatchedCartId, _sku, new CartQuantity(12), _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                _ => { Assert.Fail($"Expected: {Constants.InvalidAggregateForIdDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();
                    
                    Assert.Equal(Constants.InvalidAggregateForIdDescription, description);
                }
            );
    }
    
    [Fact]
    public void UpdateItemInCart_Should_Return_Successful_When_Item_Exists()
    {
        var existingQuantity = new CartQuantity(12);
        var quantity = new CartQuantity(12);
        UpdateItemInCartCommand command = new(_now, _customerId, _cartId, _sku, existingQuantity, _correlationId);
        var items = new List<CartItem> {new(_sku, quantity)};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Domain.Core.Version(7), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(_now, result.Aggregate.MetaData.TimeStamp);
                    Assert.Single(result.Aggregate.Items);
                    Assert.Equal((uint) 12, result.Aggregate.Items.First().Quantity.Value);
                    Assert.Equal(_sku, result.Aggregate.Items.First().Sku);
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void UpdateItemInCart_Should_Throw_Exception_When_Sku_Doesnt_Exist()
    {
        Sku invalidSku = new(Guid.NewGuid().ToString());
        UpdateItemInCartCommand command = new(_now, _customerId, _cartId, invalidSku, new CartQuantity(10), _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = _cartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<Cart, CartEvent>> result = _commandHandler.HandlerForExisting(command, aggregate);

        result
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.InvalidCartItemSkuDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.InvalidCartItemSkuCode, code);
                    Assert.Equal(Constants.InvalidCartItemSkuDescription, description);
                }
            );
    }

    [Fact]
    public void UpdateItemInCart_Should_Throw_Exception_When_Aggregate_Check_Fails()
    {
        CartId mismatchedCartId = new(Guid.NewGuid());
        UpdateItemInCartCommand command = new(_now, _customerId, _cartId, _sku, new CartQuantity(1), _correlationId);
        var items = new List<CartItem> {new(_sku, new CartQuantity(5))};
        MetaData metaData = new(new(_cartId.Value), new(6), _now);
        Cart aggregate = new Cart(_now, _customerId)
            {Id = mismatchedCartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<Cart, CartEvent>> result = _commandHandler.HandlerForExisting(command, aggregate);

        result
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.InvalidAggregateForIdDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.InvalidAggregateForIdCode, code);
                    Assert.Equal(Constants.InvalidAggregateForIdDescription, description);
                }
            );
    }

    [Fact]
    public void UpdateItemInCart_Should_Throw_Exception_When_Invalid_Command_For_New()
    {
        Sku sku = new(Guid.NewGuid().ToString());
        UpdateItemInCartCommand command = new(_now, _customerId, _cartId, sku, new CartQuantity(1), _correlationId);

        var handlerResponse = _commandHandler.HandlerForNew(command);
        
        Assert.True(handlerResponse.IsError);
        Assert.Equal(Constants.InvalidCommandForNewCode, handlerResponse.FirstError.Code);
        Assert.Equal(string.Format(Constants.InvalidCommandForNewDescription, command.GetType()), handlerResponse.FirstError.Description);
    }
}