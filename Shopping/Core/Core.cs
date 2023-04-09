namespace Shopping.Core;

public static class Constants
{
    public const string InconsistentVersionCode = "0.0";
    public const string InconsistentVersionDescription  = "Inconsistent command version: 0";
    
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
    
    public const string InvalidQuantityCode = "O.6";
    public const string InvalidQuantityDescription = "Invalid Quantity, expected Quantity > 0.";
}

public record CausationId(Guid Value);

public record CustomerId(Guid Value);

public record CommandId(Guid Value);

public record CorrelationId(Guid Value);

public record EventId(Guid Value);

public record MetaData(StreamId StreamId, Version Version, DateTime TimeStamp);

public record StreamId(Guid Value);

public record Version(uint Value);


