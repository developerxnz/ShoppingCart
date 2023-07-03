using ErrorOr;
using Shopping.Core;
using Shopping.Product.Core;
using Shopping.Product.Events;
using Shopping.Product.Persistence;

namespace Shopping.Product;

public class Transformer : Transformer<ProductAggregate, Persistence.Product>
{
    public override Persistence.Product FromDomain(ProductAggregate aggregate)//, IEnumerable<IProductEvent> events)
    {
        Shopping.Core.Persistence.Metadata metaData = new (
            aggregate.MetaData.StreamId.Value.ToString(),
            aggregate.MetaData.Version.Value,
            aggregate.MetaData.TimeStamp);

        // foreach (var @event in events)
        // {
        //     switch (@event)
        //     {
        //         case ProductCreatedEvent productCreatedEvent:
        //             var data = new ProductCreatedData(
        //                 productCreatedEvent.ProductId.Value.ToString(),
        //                 productCreatedEvent.Sku.Value,
        //                 productCreatedEvent.Description.Description,
        //                 productCreatedEvent.Price.Amount,
        //                 productCreatedEvent.CreatedOnUtc
        //                 );
        //             
        //             var eventMetadata = new EventMetadata(
        //                 productCreatedEvent.ProductId.Value.ToString(),
        //                 productCreatedEvent.CorrelationId.ToString(),
        //                 productCreatedEvent.CausationId.ToString());
        //
        //             var metadata = new Shopping.Core.Persistence.Metadata();
        //             
        //             new Persistence.ProductCreatedEventDto(data, eventMetadata, metadata);
        //     }
        //
        // }
        
        return new Persistence.Product(
            aggregate.Id.Value.ToString(), 
            aggregate.Sku.Value, 
            aggregate.Description.Description, 
            aggregate.Price.Amount, 
            aggregate.CreatedOnUtc, 
            metaData);
    }

    public override ErrorOr<ProductAggregate> ToDomain(Persistence.Product dto)
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
        Shopping.Core.Version version = new (dto.Metadata.Version);
        
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