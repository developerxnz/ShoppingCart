using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Domain.Core.Handlers;
using Shopping.Domain.Product;
using Shopping.Domain.Product.Commands;
using Shopping.Domain.Product.Events;
using Shopping.Domain.Product.Handlers;
using Shopping.Infrastructure.Interfaces;
using Shopping.Services.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Services.Products;

public sealed class Events : Service<ProductAggregate, Infrastructure.Persistence.Products.Product, ProductEvent>, IProduct
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMapper<ProductAggregate, Infrastructure.Persistence.Products.Product, IProductEvent, Infrastructure.Persistence.Products.ProductEvent> _mapper;

    public Events(IRepository<Infrastructure.Persistence.Products.Product> repository,
        IMapper<ProductAggregate, Infrastructure.Persistence.Products.Product, IProductEvent, Infrastructure.Persistence.Products.ProductEvent> mapper,
        ICommandHandler commandHandler) : base(repository)
    {
        _mapper = mapper;
        _commandHandler = commandHandler;
    }

    protected override ErrorOr<ProductAggregate> ToDomain(Infrastructure.Persistence.Products.Product aggregate)
    {
        return _mapper.ToDomain(aggregate);
    }

    protected override (Infrastructure.Persistence.Products.Product, IEnumerable<IEvent>) 
        FromDomain(ProductAggregate aggregate, IEnumerable<ProductEvent> events)
    {
        throw new NotImplementedException();
    }

    public async Task<ErrorOr<CreateProductResponse>> Create(
        CorrelationId correlationId, CancellationToken cancellationToken, CreateProductRequest request)
    {
        CreateProductCommand command = new CreateProductCommand(
            correlationId,
            DateTime.UtcNow,
            request.Sku,
            request.Description,
            request.Price);

        ErrorOr<CommandResult<ProductAggregate, ProductEvent>> commandResult = _commandHandler.HandlerForNew(command);
        return await commandResult
            .MatchAsync<ErrorOr<CreateProductResponse>>(async onValue =>
                {
                    await SaveAsync(onValue.Aggregate, onValue.Events, cancellationToken);

                    return new CreateProductResponse(onValue.Aggregate.Id, correlationId);
                },
                onError => Task.FromResult<ErrorOr<CreateProductResponse>>(ErrorOr.ErrorOr.From(onError).Errors));
    }

    public async Task<ErrorOr<UpdateProductResponse>> Update(CorrelationId correlationId,
        CancellationToken cancellationToken, UpdateProductRequest request)
    {
        PartitionKey partitionKey = new PartitionKey(request.ProductId.Value.ToString());
        Id id = new Id(request.ProductId.Value.ToString());

        var aggregateResult = await LoadAsync(partitionKey, id, cancellationToken);
        if (aggregateResult.IsError)
        {
            return ErrorOr.ErrorOr.From(aggregateResult.Errors).Value;
        }

        var command = new UpdateProductCommand(
            correlationId,
            DateTime.UtcNow,
            request.ProductId,
            request.Sku,
            request.Description,
            request.Price);

        var commandResult = _commandHandler.HandlerForExisting(command, aggregateResult.Value);
        return await commandResult
            .MatchAsync<ErrorOr<UpdateProductResponse>>(async onValue =>
                {
                    await SaveAsync(onValue.Aggregate, onValue.Events, cancellationToken);

                    return new UpdateProductResponse(onValue.Aggregate.Id, correlationId);
                },
                onError => Task.FromResult<ErrorOr<UpdateProductResponse>>(ErrorOr.ErrorOr.From(onError).Errors));
    }

    public Task<ErrorOr<UpdateProductResponse>> Get(CorrelationId correlationId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}