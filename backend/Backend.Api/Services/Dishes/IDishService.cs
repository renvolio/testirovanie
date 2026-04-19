using Backend.Api.Dtos.Dishes;

namespace Backend.Api.Services.Dishes;

public interface IDishService
{
    Task<IReadOnlyList<DishDto>> QueryAsync(
        DishListQuery query,
        CancellationToken cancellationToken = default);

    Task<DishDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DishDto> CreateAsync(CreateDishDto dto, CancellationToken cancellationToken = default);

    Task<DishDto?> UpdateAsync(Guid id, UpdateDishDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
