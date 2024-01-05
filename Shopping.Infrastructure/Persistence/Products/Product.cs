using Shopping.Domain.Core.Persistence;
using Shopping.Infrastructure.Interfaces;

namespace Shopping.Infrastructure.Persistence.Products;

public record Product(
    string Id,
    string Sku,
    string Description,
    decimal Amount,
    DateTime CreatedOnUtc,
    Metadata Metadata) : IPersistenceIdentifier { }