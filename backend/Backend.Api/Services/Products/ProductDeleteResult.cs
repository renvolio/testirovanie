namespace Backend.Api.Services.Products;

public enum ProductDeleteStatus
{
    Deleted,
    NotFound,
    BlockedByDishes
}

public readonly record struct ProductDeleteResult(
    ProductDeleteStatus Status,
    IReadOnlyList<(Guid Id, string Name)>? BlockingDishes);
