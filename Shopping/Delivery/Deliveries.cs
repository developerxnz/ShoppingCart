using Shopping.Core;
using Shopping.Delivery.Core;
using Shopping.Orders.Core;
using ErrorOr;

namespace Shopping.Delivery;

public interface IDeliveries
{

    /// <summary>
    /// Creates a new delivery for the given Order
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCreatedResponse>> CreateAsync(
        DateTime createdOnUtc, CustomerId customerId, OrderId orderId, CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCancelledResponse>> CancelAsync(
        DateTime cancelledOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Completes a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCompletedResponse>> CompleteAsync(
        DateTime completedOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken);

}

public sealed class Deliveries : Service<DeliveryAggregate, Persistence.Delivery>, IDeliveries
{
    private readonly IDeliveryCommandHandler _deliveryCommandHandler;
    private readonly ITransformer<DeliveryAggregate, Persistence.Delivery> _transformer;

    public Deliveries(
        IDeliveryCommandHandler deliveryCommandHandler, 
        IRepository<Persistence.Delivery> repository, 
        ITransformer<DeliveryAggregate, Persistence.Delivery> transformer): base(repository)
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
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new DeliveryCreatedResponse(commandResult.Value.Aggregate.Id, correlationId);
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
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new DeliveryCancelledResponse(commandResult.Value.Aggregate.Id, correlationId);
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
        
        CompleteDeliveryCommand command = new CompleteDeliveryCommand(completedOnUtc, customerId, deliveryId, orderId, correlationId);
        var commandResult = _deliveryCommandHandler.HandlerForExisting(command, aggregateResult.Value);
        if (commandResult.IsError)
        {
            return ErrorOr.ErrorOr.From(commandResult.Errors).Value;
        }
        
        await SaveAsync(commandResult.Value.Aggregate, commandResult.Value.Events, cancellationToken);

        return new DeliveryCompletedResponse(commandResult.Value.Aggregate.Id, correlationId);
    }

    protected override ErrorOr<DeliveryAggregate> ToDomain(Persistence.Delivery persistenceAggregate)
    {
        return _transformer.ToDomain(persistenceAggregate);
    }

    protected override Persistence.Delivery FromDomain(DeliveryAggregate aggregate)
    {
        return _transformer.FromDomain(aggregate);
    }
}