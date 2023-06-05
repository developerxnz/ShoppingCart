using Shopping.Domain.Core.Handlers;
using ErrorOr;
using Shopping.Core;
using Shopping.Extensions;
using Shopping.Product.Commands;
using Shopping.Product.Core;
using Shopping.Product.Events;

namespace Shopping.Product.Handlers;

public interface ICommandHandler
{
    ErrorOr<CommandResult<ProductAggregate>> HandlerForNew(IProductCommand command);

    ErrorOr<CommandResult<ProductAggregate>> HandlerForExisting(IProductCommand command, ProductAggregate aggregate);
}

public sealed class ProductCommandHandler : Handler<ProductAggregate, IProductCommand>, ICommandHandler
{
    public override ErrorOr<CommandResult<ProductAggregate>> HandlerForNew(IProductCommand command)
    {
        switch (command)
        {
            case CreateProductCommand createProductCommand:
                return GenerateEventsForProductCreated(createProductCommand);
            default:
                return Error.Unexpected(Constants.InvalidCommandForNewCode,
                    string.Format(Constants.InvalidCommandForNewDescription, command.GetType()));
        }
    }

    protected override ProductAggregate Apply(ProductAggregate aggregate, IEvent @event)
    {
        MetaData metaData = aggregate.MetaData with {Version = @event.Version, TimeStamp = @event.TimeStamp};
        return @event switch
        {
            ProductCreatedEvent x => aggregate with {CreatedOnUtc = x.CreatedOnUtc, MetaData = metaData},
            ProductUpdatedEvent x => aggregate with
            {
                Description = x.Description, Sku = x.Sku, Price = x.Price, MetaData = metaData
            },
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };
    }

    protected override ErrorOr<bool> AggregateCheck(IProductCommand command, ProductAggregate aggregate)
    {
        ProductId productId =
            (command switch
            {
                UpdateProductCommand updateProductCommand => updateProductCommand.ProductId,
                _ => throw new ArgumentOutOfRangeException(nameof(command))
            });

        if (aggregate.Id != productId)
        {
            return Error.Validation(Constants.InvalidAggregateForIdCode, Constants.InvalidAggregateForIdDescription);
        }

        return true;
    }

    protected override ErrorOr<CommandResult<ProductAggregate>> ExecuteCommand(IProductCommand command,
        ProductAggregate aggregate) =>
        (command switch
        {
            UpdateProductCommand updateProductCommand =>
                GenerateEventsForProductUpdated(updateProductCommand, aggregate),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        })
        .Match(
            commandResult => ApplyEvents(commandResult.Aggregate, commandResult.Events),
            error => ErrorOr.ErrorOr.From(error).Value);

    private ErrorOr<CommandResult<ProductAggregate>> GenerateEventsForProductCreated(CreateProductCommand command)
    {
        ProductAggregate aggregate = new(command.CreatedOnUtc);
        ProductCreatedEvent[] events =
        {
            new(
                command.CorrelationId,
                new CausationId(command.Id.Value),
                command.CreatedOnUtc,
                aggregate.Id,
                command.Description,
                command.Price,
                command.Sku,
                new Shopping.Core.Version(1))
        };

        return ApplyEvents(aggregate, events);
    }

    private ErrorOr<CommandResult<ProductAggregate>> GenerateEventsForProductUpdated(UpdateProductCommand command,
        ProductAggregate aggregate)
    {
        if (command.UpdatedOnUtc < aggregate.CreatedOnUtc)
        {
            return Error.Validation(Constants.ProductUpdatedOnBeforeCreatedOnCode,
                Constants.ProductUpdatedOnBeforeCreatedOnDescription);
        }

        return new CommandResult<ProductAggregate>(aggregate,
            new[]
            {
                new ProductUpdatedEvent(command.CorrelationId, new CausationId(command.Id.Value),
                    command.UpdatedOnUtc, command.ProductId, command.Description, command.Price, command.Sku,
                    aggregate.MetaData.Version.Increment())
            });
    }
}