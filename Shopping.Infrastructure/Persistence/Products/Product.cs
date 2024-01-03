using Shopping.Domain.Core.Persistence;
using Shopping.Infrastructure.Interfaces;
using IEvent = Shopping.Infrastructure.Interfaces.IEvent;

namespace Shopping.Infrastructure.Persistence.Products;

public abstract record ProductEvent : IEvent;

public record Product(
    string Id,
    string Sku,
    string Description,
    decimal Amount,
    DateTime CreatedOnUtc,
    Metadata Metadata) : IPersistenceIdentifier
{
    public string PartitionKey => Id;
}

public record ProductUpdatedEvent(
    string ProductId,
    string Sku,
    string Description,
    decimal Amount,
    DateTime CreatedOnUtc,
    uint Version,
    string CorrelationId,
    string CausationId)
    : ProductEvent;

public record ProductCreatedEvent(
    string ProductId,
    string Sku,
    string Description,
    decimal Amount,
    DateTime CreatedOnUtc,
    uint Version,
    string CorrelationId,
    string CausationId)
    : ProductEvent;