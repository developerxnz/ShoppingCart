using Shopping.Core;
using ErrorOr;
using Shopping.Cart;
using Shopping.Domain.Core.Handlers;
using Shopping.Extensions;
using Shopping.Product;

namespace Shopping.Delivery.Core;

public interface ICartCommandHandler
{
    ErrorOr<CommandResult<CartAggregate>> HandlerForNew(ICartCommand command);

    ErrorOr<CommandResult<CartAggregate>> HandlerForExisting(ICartCommand command, CartAggregate aggregate);
}

public class CartCommandHandler : Handler<CartAggregate>, ICartCommandHandler
{
    public ErrorOr<CommandResult<CartAggregate>> HandlerForNew(ICartCommand command)
    {
        switch (command)
        {
            case AddItemToCartCommand addItemToCartCommand:
                return GenerateEventsForItemAdded(addItemToCartCommand);
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
        }
    }

    public ErrorOr<CommandResult<CartAggregate>> HandlerForExisting(ICartCommand command, CartAggregate aggregate) =>
        aggregate.MetaData.Version switch
        {
            {Value: 0} => Error.Validation(Constants.InconsistentVersionCode, Constants.InconsistentVersionDescription),
            _ => ExecuteCommand(command, aggregate)
        };

    private ErrorOr<CommandResult<CartAggregate>> ExecuteCommand(ICartCommand command, CartAggregate aggregate) =>
        (command switch
        {
            AddItemToCartCommand addItemToCartCommand =>
                GenerateEventsForItemAdded(addItemToCartCommand, aggregate),
            RemoveItemFromCartCommand removeItemFromCartCommand =>
                GenerateEventsForItemRemoved(removeItemFromCartCommand, aggregate),
            UpdateItemInCartCommand updateItemInCartCommand =>
                GenerateEventsForItemUpdated(updateItemInCartCommand, aggregate),
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        })
        .Match(
            commandResult => ApplyEvents(commandResult.Aggregate, commandResult.Events),
            error => ErrorOr.ErrorOr.From(error).Value);

    private ErrorOr<CommandResult<CartAggregate>> GenerateEventsForItemAdded(AddItemToCartCommand command)
    {
        CartAggregate aggregate = new(command.AddedOnUtc);
        return ExecuteCommand(command, aggregate);
    }

    private ErrorOr<CommandResult<CartAggregate>> GenerateEventsForItemAdded(AddItemToCartCommand command,
        CartAggregate aggregate)
    {
        return new CommandResult<CartAggregate>(
            aggregate,
            new[]
            {
                new CartItemAddedEvent(
                    command.AddedOnUtc,
                    command.Sku,
                    command.Quantity,
                    aggregate.MetaData.Version.Increment(),
                    command.CorrelationId,
                    new(command.Id.Value))
            }
        );
    }

    private ErrorOr<CommandResult<CartAggregate>> GenerateEventsForItemRemoved(RemoveItemFromCartCommand command,
        CartAggregate aggregate)
    {
        if (aggregate.Items.All(x => x.Sku != command.Sku))
        {
            return Error.Validation(Constants.InvalidCartItemSkuCode, Constants.InvalidCartItemSkuDescription);
        }

        return new CommandResult<CartAggregate>(aggregate,
            new[]
            {
                new CartItemRemovedEvent(
                    command.RemovedOnUtc,
                    command.Sku,
                    aggregate.MetaData.Version.Increment(),
                    command.CorrelationId,
                    new(command.Id.Value))
            });
    }

    private ErrorOr<CommandResult<CartAggregate>> GenerateEventsForItemUpdated(UpdateItemInCartCommand command,
        CartAggregate aggregate)
    {
        if (aggregate.Items.All(x => x.Sku != command.Sku))
        {
            return Error.Validation(Constants.InvalidCartItemSkuCode, Constants.InvalidCartItemSkuDescription);
        }

        return new CommandResult<CartAggregate>(aggregate,
            new[]
            {
                new CartItemUpdatedEvent(
                    command.UpdatedOnUtc,
                    command.Sku,
                    command.Quantity,
                    aggregate.MetaData.Version.Increment(),
                    command.CorrelationId,
                    new(command.Id.Value))
            });
    }

    protected override CartAggregate Apply(CartAggregate aggregate, IEvent @event)
    {
        MetaData metaData = aggregate.MetaData with {Version = @event.Version, TimeStamp = @event.TimeStamp};

        aggregate = @event switch
        {
            CartItemAddedEvent x => AppendItem(aggregate, x) with { },
            CartItemRemovedEvent x => RemoveItem(aggregate, x.Sku),
            CartItemUpdatedEvent x => UpdateItem(aggregate, x),
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };

        return aggregate with {MetaData = metaData};
    }

    private CartAggregate AppendItem(CartAggregate aggregate, CartItemAddedEvent @event)
    {
        List<CartItem> items = aggregate.Items.ToList();
        items.Add(new(@event.Sku, @event.Quantity));

        return aggregate with {Items = items};
    }

    private CartAggregate RemoveItem(CartAggregate aggregate, Sku sku)
    {
        return aggregate with {Items = aggregate.Items.Where(x => x.Sku != sku).ToList()};
    }

    private CartAggregate UpdateItem(CartAggregate aggregate, CartItemUpdatedEvent @event)
    {
        List<CartItem> items = aggregate.Items.ToList();

        var updatedItem =
            items
                .Where(x => x.Sku == @event.Sku)
                .Select(x => x with {Quantity = @event.Quantity})
                .ToList();

        var x = items.Where(x => x.Sku != @event.Sku).ToList();

        return aggregate with {Items = x.Concat(updatedItem).Distinct(new CartItemComparer())};
    }

    private class CartItemComparer : IEqualityComparer<CartItem>
    {
        public bool Equals(CartItem? x, CartItem? y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Sku == y.Sku;
        }

        public int GetHashCode(CartItem cartItem)
        {
            return ReferenceEquals(cartItem, null) ? 0 : cartItem.Sku.GetHashCode();
        }
    }
}