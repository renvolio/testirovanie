using Backend.Api.Models.Enums;

namespace Backend.Api.Dtos.Dishes;

public class DishDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<string> PhotoUrls { get; set; } = Array.Empty<string>();

    /// <summary>Сохранённые ккал на порцию (черновик из расчёта или значение после ручной правки).</summary>
    public double CaloriesPerPortion { get; set; }

    /// <summary>Сохранённые белки на порцию (г).</summary>
    public double ProteinsPerPortion { get; set; }

    /// <summary>Сохранённые жиры на порцию (г).</summary>
    public double FatsPerPortion { get; set; }

    /// <summary>Сохранённые углеводы на порцию (г).</summary>
    public double CarbsPerPortion { get; set; }

    public double PortionSizeGrams { get; set; }

    public DishCategory Category { get; set; }

    public DietaryFlags AdditionalFlags { get; set; }

    /// <summary>
    /// Какие флаги можно выставить при текущем составе (все продукты должны иметь соответствующий флаг).
    /// </summary>
    public DietaryFlags AllowedAdditionalFlags { get; set; }

    public IReadOnlyList<DishCompositionItemResponseDto> Ingredients { get; set; } =
        Array.Empty<DishCompositionItemResponseDto>();

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    /// <summary>Ккал на порцию по формуле Σ (на 100 г × г в порции / 100); для сравнения с сохранённым значением при ручной правке.</summary>
    public double SuggestedCaloriesPerPortion { get; set; }

    /// <summary>Белки на порцию по той же формуле.</summary>
    public double SuggestedProteinsPerPortion { get; set; }

    /// <summary>Жиры на порцию по той же формуле.</summary>
    public double SuggestedFatsPerPortion { get; set; }

    /// <summary>Углеводы на порцию по той же формуле.</summary>
    public double SuggestedCarbsPerPortion { get; set; }
}
