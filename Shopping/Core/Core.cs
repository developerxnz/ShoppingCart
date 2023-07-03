namespace Shopping.Core;

public static class Constants
{
    public const string InconsistentVersionCode = "0.0";
    public const string InconsistentVersionDescription = "Inconsistent command version: 0";

    public const string InvalidVersionCode = "O.1";
    public const string InvalidVersionDescription = "Expected version to be 0.";

    public const string OrderAlreadyCompletedCode = "O.2";
    public const string OrderAlreadyCompletedDescription = "Order has been completed.";

    public const string OrderCancelledCode = "O.3";
    public const string OrderCancelledDescription = "Order has been cancelled.";

    public const string DeliveryAlreadyDeliveredCode = "O.4";
    public const string DeliveryAlreadyDeliveredDescription = "Delivery has been Delivered.";

    public const string InvalidCartItemSkuCode = "O.5";
    public const string InvalidCartItemSkuDescription = "Sku not found in cart.";

    public const string InvalidAggregateForIdCode = "O.6";
    public const string InvalidAggregateForIdDescription = "Aggregate Id doesn't match Command Aggregate Id.";

    public const string InvalidQuantityCode = "O.7";
    public const string InvalidQuantityDescription = "Invalid Quantity, expected Quantity > 0.";

    public const string ProductUpdatedOnBeforeCreatedOnCode = "O.8";
    public const string ProductUpdatedOnBeforeCreatedOnDescription = "Order has been completed.";

    public const string InvalidCommandForNewCode = "O.9";
    public const string InvalidCommandForNewDescription = "Invalid Command for New: {0}";

    public const string InvalidCommandForAggregateCheck = "1.0";
    public const string InvalidCommandForAggregateCheckDescription = "Invalid Command for Aggregate Check: {0}";
}

public record CausationId(Guid Value);

public record CustomerId(Guid Value)
{
    public static CustomerId Create()
    {
        return new CustomerId(Guid.NewGuid());
    }
};

public record CommandId(Guid Value);

public record CorrelationId(Guid Value)
{
    public static CorrelationId Create()
    {
        return new CorrelationId(Guid.NewGuid());
    }
}

public record EventId(Guid Value);

public record MetaData(StreamId StreamId, Version Version, DateTime TimeStamp);

public record StreamId(Guid Value);

public record Version(uint Value);

public record Total(Decimal Value);

public record TaxRate(decimal Value);

public record Tax(TaxRate TaxRate, Total Total);

public sealed record PartitionKey(string Value);

public sealed record Id(string Value);

public sealed class Paging
{
    public int PageSize { get; }

    public int CurrentPage { get; }

    public Paging(int pageSize, int currentPage)
    {
        PageSize = pageSize > 100 ? 50 : pageSize;
        CurrentPage = currentPage >= 1 ? currentPage : 1;
    }
}

public record class PagedResult<T>
{
    public T Data { get; init; }

    public int PageSize { get; init; }

    public int CurrentPage { get; init; }
}