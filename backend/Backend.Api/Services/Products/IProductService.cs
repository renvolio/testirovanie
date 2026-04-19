using Backend.Api.Dtos.Products;

namespace Backend.Api.Services.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> QueryAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default);

    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);

    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);

    Task<ProductDeleteResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
