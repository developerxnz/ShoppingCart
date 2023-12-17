using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Commands;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Delivery.Requests;
using Shopping.Domain.Orders.Core;
using Shopping.Services.Interfaces;

namespace Shopping.Services.Delivery;

public interface IDeliveries
{
    /// <summary>
    /// Creates a new delivery for the given Order
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCreatedResponse>> CreateAsync(
        DateTime createdOnUtc, CustomerId customerId, OrderId orderId, CorrelationId correlationId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cancels a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCancelledResponse>> CancelAsync(
        DateTime cancelledOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId,
        CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Completes a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCompletedResponse>> CompleteAsync(
        DateTime completedOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId,
        CorrelationId correlationId, CancellationToken cancellationToken);
}

public sealed class Deliveries : Service<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery>, IDeliveries
{
    private readonly IDeliveryCommandHandler _deliveryCommandHandler;
    private readonly ITransformer<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery> _transformer;

    public Deliveries(
        IDeliveryCommandHandler deliveryCommandHandler,
        IRepository<Infrastructure.Persistence.Delivery.Delivery> repository,
        ITransformer<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery> transformer) : base(repository)
    {
        _deliveryCommandHandler = deliveryCommandHandler;
        _transformer = transformer;
    }

    public async Task<ErrorOr<DeliveryCreatedResponse>> CreateAsync(DateTime createdOnUtc, CustomerId customerId,
        OrderId orderId,
        CorrelationId correlationId, CancellationToken cancellationToken)
    {
        CreateDeliveryCommand command = new CreateDeliveryCommand(createdOnUtc, customerId, orderId, correlationId);
        var commandResult = _deliveryCommandHandler.HandlerForNew(command);
        return await commandResult
            .MatchAsync<ErrorOr<DeliveryCreatedResponse>>(async result =>
                {
                    await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

                    return new DeliveryCreatedResponse(commandResult.Value.Aggregate.Id, correlationId);
                },
                errors =>
                {
                    ErrorOr<DeliveryCreatedResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return Task.FromResult(errorResult);
                });
    }

    public async Task<ErrorOr<DeliveryCancelledResponse>> CancelAsync(DateTime cancelledOnUtc, CustomerId customerId,
        OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(customerId.Value.ToString());
        Id id = new Id(deliveryId.Value.ToString());
        
        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }

        CancelDeliveryCommand command = new CancelDeliveryCommand(cancelledOnUtc, customerId, orderId, deliveryId, correlationId);
        var commandResult = _deliveryCommandHandler.HandlerForExisting(command, aggregateResult.Value);
        return await commandResult
            .MatchAsync<ErrorOr<DeliveryCancelledResponse>>(async result =>
                {
                    await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

                    return new DeliveryCancelledResponse(commandResult.Value.Aggregate.Id, correlationId);
                },
                errors =>
                {
                    ErrorOr<DeliveryCancelledResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return Task.FromResult(errorResult);
                });
    }

    public async Task<ErrorOr<DeliveryCompletedResponse>> CompleteAsync(DateTime completedOnUtc, CustomerId customerId,
        OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        PartitionKey partitionKey = new PartitionKey(customerId.Value.ToString());
        Id id = new Id(deliveryId.Value.ToString());
        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }

        CompleteDeliveryCommand command =
            new CompleteDeliveryCommand(completedOnUtc, customerId, deliveryId, orderId, correlationId);
        
        var commandResult = _deliveryCommandHandler.HandlerForExisting(command, aggregateResult.Value);
        return await commandResult
            .MatchAsync<ErrorOr<DeliveryCompletedResponse>>(async result =>
                {
                    await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

                    return new DeliveryCompletedResponse(commandResult.Value.Aggregate.Id, correlationId);
                },
                errors =>
                {
                    ErrorOr<DeliveryCompletedResponse> errorResult = ErrorOr.ErrorOr.From(errors).Value;
                    return Task.FromResult(errorResult);
                });
    }

    protected override ErrorOr<DeliveryAggregate> ToDomain(Infrastructure.Persistence.Delivery.Delivery aggregate)
    {
        return _transformer.ToDomain(aggregate);
    }

    protected override (Infrastructure.Persistence.Delivery.Delivery, IEnumerable<IEvent>) FromDomain(DeliveryAggregate aggregate, IEnumerable<Event> events)
    {
        throw new NotImplementedException();
    }

    // protected override Persistence.Delivery FromDomain(DeliveryAggregate aggregate)
    // {
    //     return _transformer.FromDomain(aggregate);
    // }
}