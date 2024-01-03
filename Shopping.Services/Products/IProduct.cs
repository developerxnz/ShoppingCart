using ErrorOr;
using Shopping.Domain.Core;
using Shopping.Domain.Product;

namespace Shopping.Services.Products;

public interface IProduct
{
    /// <summary>
    /// Creates a new Product
    /// </summary>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ErrorOr<CreateProductResponse>> Create(
        CorrelationId correlationId,
        CancellationToken cancellationToken,
        CreateProductRequest request);

    /// <summary>
    /// Update a Product
    /// </summary>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<ErrorOr<UpdateProductResponse>> Update(
        CorrelationId correlationId,
        CancellationToken cancellationToken,
        UpdateProductRequest request);

    /// <summary>
    /// Update a Product
    /// </summary>
    /// <param name="correlationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<UpdateProductResponse>> Get(
        CorrelationId correlationId,
        CancellationToken cancellationToken);
}