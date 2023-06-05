using Shopping.Cart.Projections;
using Shopping.Core;
using Shopping.Product.Core;

namespace Shopping.Product;

public record CreateProductRequest(Sku Sku, ProductDescription Description, ProductPrice Price);

public record CreateProductResponse(ProductId ProductId, CorrelationId CorrelationId);

public record UpdateProductRequest(ProductId ProductId, Sku Sku, ProductDescription Description, ProductPrice Price);

public record UpdateProductResponse(ProductId ProductId, CorrelationId CorrelationId);

