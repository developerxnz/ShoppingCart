using ErrorOr;
using Shopping.Core;
using Shopping.Extensions;
using Shopping.Orders;
using Shopping.Orders.Commands;
using Shopping.Orders.Core;
using Shopping.Orders.Events;
using Shopping.Orders.Handlers;

namespace ShoppingUnitTests;

using Shopping;

public class OrderHandlerUnitTest
{
    private readonly ICommandHandler _commandHandler;

    public OrderHandlerUnitTest()
    {
        _commandHandler = new CommandHandler();
    }

    [Fact]
    public void CreateOrder_Should_Return_Successful_when_aggregate_valid()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = new(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        CreateOrderCommand command = new CreateOrderCommand(createdOnUtc, customerId, correlationId);

        _commandHandler.HandlerForNew(command)
            .Switch(
                result =>
                {
                    Assert.Equal(new Shopping.Core.Version(1), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(customerId.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(createdOnUtc, result.Aggregate.MetaData.TimeStamp);
                },
                errors => Assert.Fail("Expected Order")
            );
    }

    [Fact]
    public void CompleteOrder_Should_Return_Aggregate_And_Events()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(1);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId) {Id = orderId, MetaData = metaData};
        DateTime completedOnUtc = DateTime.UtcNow;

        var command = new CompleteOrderCommand(completedOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                result =>
                {
                    Assert.Equal(version.Increment(), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(orderId.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(completedOnUtc, result.Aggregate.CompletedOnUtc);
                    Assert.Equal(completedOnUtc, result.Events.First().TimeStamp);
                },
                errors => { Assert.Fail($"Expected Order Aggregate: {errors.First().Description}"); }
            );
    }

    [Fact]
    public void CompleteOrder_Should_Fail_When_Already_Competed()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(1);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId)
            {Id = orderId, CompletedOnUtc = DateTime.UtcNow, MetaData = metaData};
        DateTime completedOnUtc = DateTime.UtcNow;

        var command = new CompleteOrderCommand(completedOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.OrderAlreadyCompletedDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.OrderAlreadyCompletedCode, code);
                    Assert.Equal(Constants.OrderAlreadyCompletedDescription, description);
                }
            );
    }

    [Fact]
    public void CompleteOrder_Should_Fail_When_Cancelled()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(1);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId)
            {Id = orderId, CancelledOnUtc = DateTime.UtcNow, MetaData = metaData};
        DateTime completedOnUtc = DateTime.UtcNow;

        var command = new CompleteOrderCommand(completedOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.OrderAlreadyCompletedDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.OrderCancelledCode, code);
                    Assert.Equal(Constants.OrderCancelledDescription, description);
                }
            );
    }

    [Fact]
    public void CompleteOrder_Should_Fail_When_Version_Is_0()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(0);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId) {Id = orderId};
        DateTime completedOnUtc = DateTime.UtcNow;

        var command = new CompleteOrderCommand(completedOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.InvalidVersionDescription}"); },
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
    public void CancelOrder_Should_Return_Aggregate_And_Events()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(1);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId) {Id = orderId, MetaData = metaData};
        DateTime cancelledOnUtc = DateTime.UtcNow;

        var command = new CancelOrderCommand(cancelledOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                result =>
                {
                    Assert.Equal(version.Increment(), result.Aggregate.MetaData.Version);
                    Assert.Equal(new(orderId.Value), result.Aggregate.MetaData.StreamId);
                    Assert.Equal(cancelledOnUtc, result.Aggregate.CancelledOnUtc);
                    Assert.Equal(cancelledOnUtc, result.Events.First().TimeStamp);
                },
                errors => { Assert.Fail($"Expected Order Aggregate: {errors.First().Description}"); }
            );
    }

    [Fact]
    public void CancelOrder_Should_Fail_When_Order_Cancelled()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(1);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId)
            {Id = orderId, CancelledOnUtc = DateTime.UtcNow, MetaData = metaData};
        DateTime cancelledOnUtc = DateTime.UtcNow;

        var command = new CancelOrderCommand(cancelledOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.OrderCancelledDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.OrderCancelledCode, code);
                    Assert.Equal(Constants.OrderCancelledDescription, description);
                }
            );
    }

    [Fact]
    public void CancelOrder_Should_Fail_When_Order_Completed()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(1);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId)
            {Id = orderId, CompletedOnUtc = DateTime.UtcNow, MetaData = metaData};
        DateTime cancelledOnUtc = DateTime.UtcNow;

        var command = new CancelOrderCommand(cancelledOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.OrderAlreadyCompletedDescription}"); },
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.OrderAlreadyCompletedCode, code);
                    Assert.Equal(Constants.OrderAlreadyCompletedDescription, description);
                }
            );
    }

    [Fact]
    public void CancelOrder_Should_Fail_When_Version_Is_0()
    {
        CustomerId customerId = new CustomerId(Guid.NewGuid());
        OrderId orderId = new OrderId(Guid.NewGuid());
        CorrelationId correlationId = new(Guid.NewGuid());
        Shopping.Core.Version version = new(0);
        MetaData metaData = new(new(orderId.Value), version, DateTime.UtcNow);
        OrderAggregate order = new OrderAggregate(DateTime.UtcNow, customerId) {Id = orderId};
        DateTime cancelledOnUtc = DateTime.UtcNow;

        var command = new CancelOrderCommand(cancelledOnUtc, customerId, orderId, correlationId);

        _commandHandler.HandlerForExisting(command, order)
            .Switch(
                _ => { Assert.Fail($"Expected {Constants.InvalidVersionDescription}"); },
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
}