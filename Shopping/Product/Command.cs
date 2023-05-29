using Shopping.Cart.Projections;
using Shopping.Core;
using Shopping.Domain.Core.Handlers;

namespace Shopping.Product;

public interface IProductCommand : ICommand {}

public record ProductCommand<T>(CorrelationId CorrelationId, T Data) : Command<T>(CorrelationId, Data), IProductCommand;

public record CreateProductData(
    DateTime CreatedOnUtc,
    Sku Sku,
    ProductDescription Description,
    ProductPrice Price);

public record CreateProductCommand(
        CorrelationId CorrelationId, 
        DateTime CreatedOnUtc,
        Sku Sku,
        ProductDescription Description,
        ProductPrice Price)
    : ProductCommand<CreateProductData>(
        CorrelationId,
        new CreateProductData(CreatedOnUtc, Sku, Description, Price)
    );
    
public record UpdateProductData(
    DateTime UpdatedOnUtc, 
    ProductId ProductId,
    Sku Sku,
    ProductDescription Description,
    ProductPrice Price);
    
public record UpdateProductCommand(
        CorrelationId CorrelationId, 
        DateTime UpdatedOnUtc,
        ProductId ProductId,
        Sku Sku,
        ProductDescription Description,
        ProductPrice Price)
    : ProductCommand<UpdateProductData>(
        CorrelationId,
        new UpdateProductData(UpdatedOnUtc, ProductId, Sku, Description, Price)
    );