using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;
using Version = Shopping.Domain.Core.Version;

namespace Shopping.Infrastructure.Persistence.Products;

public static class ProductEventExtensions
{
    public static ProductCreatedEvent FromDomain(this Domain.Product.Events.ProductCreatedEvent domain)
    {
        return new ProductCreatedEvent(
            domain.ProductId.Value.ToString(), 
            domain.Sku.Value, 
            domain.Description.Description,
            domain.Price.Amount, 
            domain.CreatedOnUtc,
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static Domain.Product.Events.ProductCreatedEvent ToDomain(this ProductCreatedEvent dto)
    {
        ProductId productId = new ProductId(Guid.Parse(dto.ProductId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        Version version = new Version(dto.Version);
        ProductDescription description = new ProductDescription(dto.Description);
        ProductPrice price = new ProductPrice(dto.Amount);
        Sku sku = new Sku(dto.Sku);

        return new Domain.Product.Events.ProductCreatedEvent(
            correlationId,
            causationId,
            dto.CreatedOnUtc,
            productId,
            description,
            price,
            sku,
            version
        );
    }
    
    public static ProductUpdatedEvent FromDomain(this Domain.Product.Events.ProductUpdatedEvent domain)
    {
        return new ProductUpdatedEvent(
            domain.ProductId.Value.ToString(), 
            domain.Sku.Value, 
            domain.Description.Description,
            domain.Price.Amount, 
            domain.UpdatedOnUtc,
            domain.Version.Value,
            domain.CorrelationId.Value.ToString(),
            domain.CausationId.Value.ToString());
    }

    public static Domain.Product.Events.ProductUpdatedEvent ToDomain(this ProductUpdatedEvent dto)
    {
        ProductId productId = new ProductId(Guid.Parse(dto.ProductId));
        CausationId causationId = new CausationId(Guid.Parse(dto.CausationId));
        CorrelationId correlationId = new CorrelationId(Guid.Parse(dto.CorrelationId));
        Version version = new Version(dto.Version);
        ProductDescription description = new ProductDescription(dto.Description);
        ProductPrice price = new ProductPrice(dto.Amount);
        Sku sku = new Sku(dto.Sku);

        return new Domain.Product.Events.ProductUpdatedEvent(
            correlationId,
            causationId,
            dto.CreatedOnUtc,
            productId,
            description,
            price,
            sku,
            version
        );
    }

}