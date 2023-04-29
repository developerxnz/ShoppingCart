using ErrorOr;
using Shopping.Cart;
using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product;

namespace ShoppingUnitTests;

public class CartHandlerTests
{
    private readonly ICartCommandHandler _commandHandler;

    public CartHandlerTests()
    {
        _commandHandler = new CartCommandHandler();
    }

    [Fact]
    public void AddItemToCart_New_Cart_Should_Return_Successful()
    {
        DateTime addedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        AddItemToCartCommand command = new(addedOnUtc, customerId, null, sku, 1, correlationId);

        _commandHandler.HandlerForNew(command)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Core.Version(1), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(addedOnUtc, result.Aggregate.MetaData.TimeStamp);
                    Assert.Single(result.Aggregate.Items);
                    Assert.Equal(addedOnUtc, result.Aggregate.CreatedOnUtc);
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void AddItemToCart_Existing_Cart_Should_Fail_When_Version_Is_0()
    {
        DateTime addedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        CartAggregate aggregate = new CartAggregate(addedOnUtc, customerId);
        AddItemToCartCommand command = new(addedOnUtc, customerId, aggregate.Id, sku, 1, correlationId);
        
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
        DateTime addedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        CorrelationId correlationId = new CorrelationId(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        CartId unMatchedCartId = new CartId(Guid.NewGuid());
        AddItemToCartCommand command = new AddItemToCartCommand(addedOnUtc, customerId, unMatchedCartId, sku, 1, correlationId);
        CartAggregate aggregate = new CartAggregate(addedOnUtc, customerId);
        var versionedAggregate = aggregate with { MetaData = aggregate.MetaData with { Version = new Shopping.Core.Version(2) } };

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
        DateTime addedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        AddItemToCartCommand command = new(addedOnUtc, customerId, null, sku, 1, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), addedOnUtc);
        CartAggregate aggregate = new CartAggregate(addedOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Core.Version(7), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(addedOnUtc, result.Aggregate.MetaData.TimeStamp);
                    Assert.True(result.Aggregate.Items.Count() == 2);
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void AddItemToCart_Existing_Cart_Should_Fail_when_aggregate_check_fails()
    {
        DateTime addedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CartId mismatchedCartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        AddItemToCartCommand command = new(addedOnUtc, customerId, cartId, sku, 1, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), addedOnUtc);
        CartAggregate aggregate = new CartAggregate(addedOnUtc, customerId)
            {Id = mismatchedCartId, Items = items, MetaData = metaData};

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
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime removedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        RemoveItemFromCartCommand command = new(removedOnUtc, customerId, cartId, sku, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), removedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Core.Version(7), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(removedOnUtc, result.Aggregate.MetaData.TimeStamp);
                    Assert.True(!result.Aggregate.Items.Any());
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void RemoveItemFromCart_Should_Fail_when_aggregate_check_fails()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime removedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CartId mismatchedCartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        RemoveItemFromCartCommand command = new(removedOnUtc, customerId, mismatchedCartId, sku, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), removedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};

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
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime removedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        Sku invalidSku = new(Guid.NewGuid());
        RemoveItemFromCartCommand command = new(removedOnUtc, customerId, cartId, invalidSku, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), removedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<CartAggregate>> result = _commandHandler.HandlerForExisting(command, aggregate);

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
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime removedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CartId mismatchedCartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        Sku invalidSku = new(Guid.NewGuid());
        RemoveItemFromCartCommand command = new(removedOnUtc, customerId, cartId, invalidSku, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), removedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = mismatchedCartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<CartAggregate>> result = _commandHandler.HandlerForExisting(command, aggregate);

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
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime updatedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CartId mismatchedCartId = new CartId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        UpdateItemInCartCommand command = new(updatedOnUtc, customerId, mismatchedCartId, sku, 12, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), updatedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};

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
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime updatedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        UpdateItemInCartCommand command = new(updatedOnUtc, customerId, cartId, sku, 12, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), updatedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};

        _commandHandler.HandlerForExisting(command, aggregate)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Core.Version(7), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(result.Aggregate.Id.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(updatedOnUtc, result.Aggregate.MetaData.TimeStamp);
                    Assert.Single(result.Aggregate.Items);
                    Assert.Equal((uint) 12, result.Aggregate.Items.First().Quantity);
                    Assert.Equal(sku, result.Aggregate.Items.First().Sku);
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void UpdateItemInCart_Should_Throw_Exception_When_Sku_Doesnt_Exist()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime updatedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        Sku invalidSku = new(Guid.NewGuid());
        UpdateItemInCartCommand command = new(updatedOnUtc, customerId, cartId, invalidSku, 10, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), updatedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<CartAggregate>> result = _commandHandler.HandlerForExisting(command, aggregate);

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
    public void UpdateItemInCart_Should_Throw_Exception_When_Invalid_Quantity()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime updatedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        UpdateItemInCartCommand command = new(updatedOnUtc, customerId, cartId, sku, 0, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), updatedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = cartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<CartAggregate>> result = _commandHandler.HandlerForExisting(command, aggregate);

        result
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.InvalidQuantityDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.InvalidQuantityCode, code);
                    Assert.Equal(Constants.InvalidQuantityDescription, description);
                }
            );
    }

    [Fact]
    public void UpdateItemInCart_Should_Throw_Exception_When_Aggregate_Check_Fails()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime updatedOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        CartId mismatchedCartId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        UpdateItemInCartCommand command = new(updatedOnUtc, customerId, cartId, sku, 0, correlationId);
        var items = new List<CartItem> {new(sku, 5)};
        MetaData metaData = new(new(cartId.Value), new(6), updatedOnUtc);
        CartAggregate aggregate = new CartAggregate(createdOnUtc, customerId)
            {Id = mismatchedCartId, Items = items, MetaData = metaData};
        ErrorOr<CommandResult<CartAggregate>> result = _commandHandler.HandlerForExisting(command, aggregate);

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
        DateTime addedOnUtc = DateTime.UtcNow;
        CorrelationId correlationId = new(Guid.NewGuid());
        CustomerId customerId = new(Guid.NewGuid());
        CartId cartId = new(Guid.NewGuid());
        Sku sku = new(Guid.NewGuid());
        UpdateItemInCartCommand command = new(addedOnUtc, customerId, cartId, sku, 1, correlationId);

        Assert.Throws<ArgumentOutOfRangeException>(() => _commandHandler.HandlerForNew(command));
    }
}