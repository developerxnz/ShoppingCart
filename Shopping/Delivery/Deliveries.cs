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
    Task<ErrorOr<DeliveryCreatedResponse>> Create(
        DateTime createdOnUtc, CustomerId customerId, OrderId orderId, CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Cancels a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCancelledResponse>> Cancel(
        DateTime cancelledOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken);

    /// <summary>
    /// Completes a delivery
    /// </summary>
    /// <returns></returns>
    Task<ErrorOr<DeliveryCompletedResponse>> Complete(
        DateTime completedOnUtc, CustomerId customerId, OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken);

}

public sealed class Deliveries : IDeliveries
{
    private readonly IDeliveryCommandHandler _deliveryCommandHandler;
    private readonly IRepository<Persistence.Delivery> _repository;
    private readonly ITransformer<DeliveryAggregate, Persistence.Delivery> _transformer;

    public Deliveries(
        IDeliveryCommandHandler deliveryCommandHandler, 
        IRepository<Persistence.Delivery> repository, 
        ITransformer<DeliveryAggregate, Persistence.Delivery> transformer)
    {
        _deliveryCommandHandler = deliveryCommandHandler;
        _repository = repository;
        _transformer = transformer;
    }

    public async Task<ErrorOr<DeliveryCreatedResponse>> Create(DateTime createdOnUtc, CustomerId customerId,
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

    public async Task<ErrorOr<DeliveryCancelledResponse>> Cancel(DateTime cancelledOnUtc, CustomerId customerId,
        OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        var aggregateResult = await LoadAsync(customerId, deliveryId, cancellationToken);
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

    public async Task<ErrorOr<DeliveryCompletedResponse>> Complete(DateTime completedOnUtc, CustomerId customerId,
        OrderId orderId, DeliveryId deliveryId, CorrelationId correlationId, CancellationToken cancellationToken)
    {
        var aggregateResult = await LoadAsync(customerId, deliveryId, cancellationToken);
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
    
    private async Task<ErrorOr<DeliveryAggregate>> LoadAsync(CustomerId customerId, DeliveryId deliveryId, CancellationToken cancellationToken)
    {
        Persistence.Delivery response = await _repository.GetByIdAsync(customerId.Value.ToString(), deliveryId.Value.ToString(), cancellationToken);
        return _transformer.ToDomain(response);
    }
    
    private async Task SaveAsync(DeliveryAggregate aggregate, IEnumerable<Event> events, CancellationToken cancellationToken)
    {
        var transformedAggregate = _transformer.FromDomain(aggregate);
        await _repository.BatchUpdateAsync(transformedAggregate.OrderId, transformedAggregate, events);
    }
}