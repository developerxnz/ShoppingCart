using ErrorOr;
using Shopping.Core;
using Shopping.Domain.Core.Handlers;
using Shopping.Product.Commands;
using Shopping.Product.Handlers;

namespace Shopping.Product.Services;

public interface IProduct
{
    /// <summary>
    /// Creates a new Product
    /// </summary>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ErrorOr<CreateProductResponse>> Create(
        CorrelationId correlationId,
        CancellationToken cancellationToken,
        CreateProductRequest request);

    /// <summary>
    /// Update a Product
    /// </summary>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ErrorOr<UpdateProductResponse>> Update(
        CorrelationId correlationId,
        CancellationToken cancellationToken,
        UpdateProductRequest request);
}

public sealed class Product : Service<ProductAggregate, Shopping.Product.Persistence.Product>, IProduct
{
    private readonly ICommandHandler _commandHandler;
    private readonly ITransformer<ProductAggregate, Persistence.Product> _transformer;

    public Product(IRepository<Shopping.Product.Persistence.Product> repository,
        ITransformer<ProductAggregate, Persistence.Product> transformer,
        ICommandHandler commandHandler) : base(repository)
    {
        _transformer = transformer;
        _commandHandler = commandHandler;
    }

    protected override ErrorOr<ProductAggregate> ToDomain(Shopping.Product.Persistence.Product aggregate)
    {
        return _transformer.ToDomain(aggregate);
    }

    protected override Shopping.Product.Persistence.Product FromDomain(ProductAggregate aggregate)
    {
        return _transformer.FromDomain(aggregate);
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

        ErrorOr<CommandResult<ProductAggregate>> commandResult = _commandHandler.HandlerForNew(command);
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
}