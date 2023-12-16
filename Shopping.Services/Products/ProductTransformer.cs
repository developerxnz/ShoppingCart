using ErrorOr;
using Shopping.Core;
using Shopping.Infrastructure.Persistence.Products;
using Shopping.Product;
using Shopping.Product.Core;
using Shopping.Product.Events;

namespace Shopping.Services.Products;

public class ProductTransformer : Transformer<ProductAggregate, Infrastructure.Persistence.Products.Product>, 
    ITransformer<ProductAggregate, Infrastructure.Persistence.Products.Product>
{
    public (object, IEnumerable<object>) FromDomain(ProductAggregate aggregate,
        IEnumerable<IProductEvent> events)
    {
        var converted = new List<object>();
        foreach (var @event in events)
        {
            switch (@event)
            {
                case ProductCreatedEvent productCreatedEvent:
                    var data = new ProductCreatedData(
                        productCreatedEvent.ProductId.Value.ToString(),
                        productCreatedEvent.Sku.Value,
                        productCreatedEvent.Description.Description,
                        productCreatedEvent.Price.Amount,
                        productCreatedEvent.CreatedOnUtc
                    );

                    var eventMetadata = new EventMetadata(
                        productCreatedEvent.ProductId.Value.ToString(),
                        productCreatedEvent.CorrelationId.ToString(),
                        productCreatedEvent.CausationId.ToString());

                    var metadata = new Shopping.Core.Persistence.Metadata(
                        productCreatedEvent.ProductId.Value.ToString(),
                        productCreatedEvent.Version.Value,
                        productCreatedEvent.TimeStamp);

                    converted.Add(new ProductCreatedEventDto(data, eventMetadata, metadata));
                    break;
            }
        }

        Shopping.Core.Persistence.Metadata metaData = new(
            aggregate.MetaData.StreamId.Value.ToString(),
            aggregate.MetaData.Version.Value,
            aggregate.MetaData.TimeStamp);

        return (new Infrastructure.Persistence.Products.Product(
            aggregate.Id.Value.ToString(),
            aggregate.Sku.Value,
            aggregate.Description.Description,
            aggregate.Price.Amount,
            aggregate.CreatedOnUtc,
            metaData) as object, converted);
    }

    public override Infrastructure.Persistence.Products.Product FromDomain(ProductAggregate aggregate)
    {
        Shopping.Core.Persistence.Metadata metaData = new(
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
        Shopping.Core.Version version = new(dto.Metadata.Version);
        
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
}