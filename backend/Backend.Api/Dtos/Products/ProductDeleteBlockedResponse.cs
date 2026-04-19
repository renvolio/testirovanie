namespace Backend.Api.Dtos.Products;

/// <summary>Ответ при отказе в удалении продукта из‑за использования в блюдах.</summary>
public class ProductDeleteBlockedResponse
{
    public string Message { get; set; } =
        "Удаление невозможно: продукт входит в состав блюд. Сначала измените состав этих блюд.";

    public IReadOnlyList<ProductBlockingDishRefDto> Dishes { get; set; } = Array.Empty<ProductBlockingDishRefDto>();
}

public class ProductBlockingDishRefDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
