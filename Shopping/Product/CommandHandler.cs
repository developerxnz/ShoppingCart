using Shopping.Domain.Core.Handlers;
using ErrorOr;
using Shopping.Core;

namespace Shopping.Product.Handlers;

public interface IProductCommandHandler
{
    ErrorOr<CommandResult<ProductAggregate>> HandlerForNew(IProductCommand command);

    ErrorOr<CommandResult<ProductAggregate>> HandlerForExisting(IProductCommand command, ProductAggregate aggregate);
}

public sealed class ProductCommandHandler : Handler<ProductAggregate, IProductCommand>, IProductCommandHandler
{
    public override ErrorOr<CommandResult<ProductAggregate>> HandlerForNew(IProductCommand command)
    {
        throw new NotImplementedException();
    }

    protected override ProductAggregate Apply(ProductAggregate aggregate, IEvent @event)
    {
        throw new NotImplementedException();
    }

    protected override ErrorOr<bool> AggregateCheck(IProductCommand command, ProductAggregate aggregate)
    {
        throw new NotImplementedException();
    }

    protected override ErrorOr<CommandResult<ProductAggregate>> ExecuteCommand(IProductCommand command, ProductAggregate aggregate)
    {
        throw new NotImplementedException();
    }
}