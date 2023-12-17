using Shopping.Domain.Core;
using Shopping.Domain.Product.Core;

namespace Shopping.Domain.Product;

public record CreateProductRequest(Sku Sku, ProductDescription Description, ProductPrice Price);

public record CreateProductResponse(ProductId ProductId, CorrelationId CorrelationId);

public record UpdateProductRequest(ProductId ProductId, Sku Sku, ProductDescription Description, ProductPrice Price);

public record UpdateProductResponse(ProductId ProductId, CorrelationId CorrelationId);

