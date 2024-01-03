using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Delivery.Commands;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Delivery.Events;
using Shopping.Domain.Delivery.Requests;
using Shopping.Domain.Orders.Core;
using Shopping.Infrastructure.Interfaces;
using Shopping.Services.Interfaces;
using DeliveryEvent = Shopping.Infrastructure.Persistence.Delivery.DeliveryEvent;

namespace Shopping.Services.Delivery;

public sealed class Deliveries : Service<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery, Domain.Delivery.Events.DeliveryEvent>, IDeliveries
{
    private readonly IDeliveryCommandHandler _deliveryCommandHandler;
    private readonly IMapper<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery, IDeliveryEvent, DeliveryEvent> _mapper;

    public Deliveries(
        IDeliveryCommandHandler deliveryCommandHandler,
        IRepository<Infrastructure.Persistence.Delivery.Delivery> repository,
        IMapper<DeliveryAggregate, Infrastructure.Persistence.Delivery.Delivery, IDeliveryEvent, DeliveryEvent> mapper) 
        : base(repository)
    {
        _deliveryCommandHandler = deliveryCommandHandler;
        _mapper = mapper;
    }

    public async Task<ErrorOr<DeliveryCreatedResponse>> CreateAsync(DateTime createdOnUtc, CustomerId customerId,
        OrderId orderId, CorrelationId correlationId, CancellationToken cancellationToken)
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
        return _mapper.ToDomain(aggregate);
    }

    protected override (Infrastructure.Persistence.Delivery.Delivery, IEnumerable<Infrastructure.Interfaces.IEvent>) 
        FromDomain(DeliveryAggregate aggregate, IEnumerable<Domain.Delivery.Events.DeliveryEvent> events)
    {
        throw new NotImplementedException();
    }
}