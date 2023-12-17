using Shopping.Domain.Core;
using Moq;
using ErrorOr;
using Shopping.Domain.Delivery.Core;
using Shopping.Domain.Delivery.Requests;
using Shopping.Domain.Domain.Core.Handlers;
using Shopping.Domain.Orders.Core;
using Shopping.Services.Delivery;
using Shopping.Services.Interfaces;

namespace ShoppingUnitTests.Delivery;

public class DeliveryServiceTests
{
    private readonly Mock<IDeliveryCommandHandler> _deliveryCommandHandler;
    private readonly Mock<IRepository<Shopping.Infrastructure.Persistence.Delivery.Delivery>> _repository;
    private readonly ITransformer<DeliveryAggregate, Shopping.Infrastructure.Persistence.Delivery.Delivery> _transformer;
    private readonly IDeliveries _service;

    public DeliveryServiceTests()
    {
        _deliveryCommandHandler = new Mock<IDeliveryCommandHandler>();
        _repository = new Mock<IRepository<Shopping.Infrastructure.Persistence.Delivery.Delivery>>();
        _transformer = new DeliveryTransformer();
        _service = new Deliveries(_deliveryCommandHandler.Object, _repository.Object, _transformer);
    }

    [Fact]
    public async Task Create_Delivery_Should_Return_Successful()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        CustomerId customerId = CustomerId.Create();
        OrderId orderId = OrderId.Create();
        CorrelationId correlationId = CorrelationId.Create();
        CancellationToken cancellationToken = new CancellationToken();
        DeliveryAggregate aggregate = new DeliveryAggregate(createdOnUtc, orderId);
        ErrorOr<CommandResult<DeliveryAggregate>> commandResult =
            new CommandResult<DeliveryAggregate>(aggregate, Enumerable.Empty<Event>());

        _deliveryCommandHandler
            .Setup(x => x.HandlerForNew(It.IsAny<IDeliveryCommand>()))
            .Returns(commandResult);

        var response =
            await _service
                .CreateAsync(createdOnUtc, customerId, orderId, correlationId, cancellationToken);

        response
            .Switch(
                deliveryCreatedResponse => { Assert.Equal(correlationId, deliveryCreatedResponse.CorrelationId); },
                errors =>
                    Assert.Fail($"Expected {nameof(CreateDeliveryResponse)}")
            );
    }

    [Fact]
    public async Task Complete_Delivery_Should_Return_Error_When_Already_Delivered()
    {
        DateTime createdOnUtc = DateTime.UtcNow;
        DateTime completedOnUtc = DateTime.UtcNow;
        CustomerId customerId = CustomerId.Create();
        OrderId orderId = OrderId.Create();
        DeliveryId deliveryId = DeliveryId.Create();
        CorrelationId correlationId = CorrelationId.Create();
        CancellationToken cancellationToken = new CancellationToken();
        StreamId streamId = new StreamId(Guid.NewGuid());
        Shopping.Infrastructure.Persistence.Delivery.Delivery dto = 
            new(
                deliveryId.Value.ToString(),
                createdOnUtc,
                completedOnUtc,
                new Shopping.Domain.Core.Persistence.Metadata(streamId.Value.ToString(), 1, createdOnUtc ),
                orderId.Value.ToString());
        
        ErrorOr<CommandResult<DeliveryAggregate>> commandResult = ErrorOr<CommandResult<DeliveryAggregate>>
            .From(new List<Error>
            {
                Error.Validation(Constants.DeliveryAlreadyDeliveredCode, Constants.DeliveryAlreadyDeliveredDescription)
            });

        _deliveryCommandHandler
            .Setup(x => x.HandlerForExisting(It.IsAny<IDeliveryCommand>(), It.IsAny<DeliveryAggregate>()))
            .Returns(commandResult);

        _repository
            .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        
        var response =
            await _service
                .CompleteAsync(completedOnUtc, customerId, orderId, deliveryId, correlationId, cancellationToken);

        response
            .Switch(
                deliveryCreatedResponse => Assert.Fail($"Expected {Constants.DeliveryAlreadyDeliveredDescription}"),
                errors =>
                {
                    var (code, description) =
                        errors
                            .Where(x => x.Type == ErrorType.Validation)
                            .Select(x => (x.Code, x.Description))
                            .First();

                    Assert.Equal(Constants.DeliveryAlreadyDeliveredCode, code);
                    Assert.Equal(Constants.DeliveryAlreadyDeliveredDescription, description);
                });
    }
}