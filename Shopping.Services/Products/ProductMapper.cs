using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Product;
using Shopping.Domain.Product.Core;
using Shopping.Domain.Product.Events;
using Shopping.Infrastructure.Persistence.Products;
using Shopping.Services.Interfaces;
using ProductCreatedEvent = Shopping.Domain.Product.Events.ProductCreatedEvent;
using ProductEvent = Shopping.Infrastructure.Persistence.Products.ProductEvent;
using ProductUpdatedEvent = Shopping.Domain.Product.Events.ProductUpdatedEvent;

namespace Shopping.Services.Products;

public class ProductMapper : 
    Mapper<ProductAggregate, Infrastructure.Persistence.Products.Product, IProductEvent, Infrastructure.Persistence.Products.ProductEvent>,
    IMapper<ProductAggregate, Infrastructure.Persistence.Products.Product, IProductEvent, Infrastructure.Persistence.Products.ProductEvent>
{
    public override Infrastructure.Persistence.Products.Product FromDomain(ProductAggregate aggregate)
    {
        Domain.Core.Persistence.Metadata metaData = new(
            aggregate.MetaData.StreamId.Value.ToString(),
            aggregate.MetaData.Version.Value,
            aggregate.MetaData.TimeStamp);

        return new Infrastructure.Persistence.Products.Product(
            aggregate.Id.Value.ToString(),
            aggregate.Sku.Value,
            aggregate.Description.Description,
            aggregate.Price.Amount,
            aggregate.CreatedOnUtc,
            metaData);
    }

    public override ProductEvent FromDomain(IProductEvent @event)
        => @event switch
        {
            ProductCreatedEvent productCreatedEvent => productCreatedEvent.FromDomain(),
            ProductUpdatedEvent productUpdatedEvent => productUpdatedEvent.FromDomain(),
            _ => throw new ArgumentOutOfRangeException(nameof(@event))
        };

    public (Infrastructure.Persistence.Products.Product, IEnumerable<ProductEvent>) FromDomain(ProductAggregate aggregate, IEnumerable<IProductEvent> events)
    {
        throw new NotImplementedException();
    }

    public override ErrorOr<ProductAggregate> ToDomain(Infrastructure.Persistence.Products.Product dto)
    {
        if (!Guid.TryParse(dto.Id, out var productIdGuid))
        {
            return Error.Validation($"Invalid ProductId: {dto.Id}");
        }
        
        if (!Guid.TryParse(dto.Id, out var streamIdGuid))
        {
            return Error.Validation($"Invalid StreamId: {dto.Metadata.StreamId}");
        }
        
        ProductId productId = new(productIdGuid);
        ProductPrice price = new ProductPrice(dto.Amount);
        StreamId streamId = new StreamId(streamIdGuid);
        Sku sku = new Sku(dto.Sku);
        Domain.Core.Version version = new(dto.Metadata.Version);
        
        MetaData metaData =
            new MetaData(
                streamId,
                version,
                dto.Metadata.Timestamp);
        
        return new ProductAggregate(dto.CreatedOnUtc)
        {
            Description = new(dto.Description),
            Price = price,
            Sku = sku,
            MetaData = metaData,
            Id = productId
        };
    }

    public override ErrorOr<IProductEvent> ToDomain(ProductEvent dto)
    {
        throw new NotImplementedException();
    }
}