using Backend.Api.Models.Enums;

namespace Backend.Api.Models.Entities;

/// <summary>Блюдо.</summary>
public class Dish
{
    public Guid Id { get; set; }

    /// <summary>Наименование блюда, минимум 2 символа.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ссылки или пути к изображениям (0–5 штук).</summary>
    public List<string> PhotoUrls { get; set; } = new();

    /// <summary>Ккал на одну порцию.</summary>
    public double CaloriesPerPortion { get; set; }

    /// <summary>Белки на одну порцию, г.</summary>
    public double ProteinsPerPortion { get; set; }

    /// <summary>Жиры на одну порцию, г.</summary>
    public double FatsPerPortion { get; set; }

    /// <summary>Углеводы на одну порцию, г.</summary>
    public double CarbsPerPortion { get; set; }

    /// <summary>Масса одной порции блюда, г (&gt; 0).</summary>
    public double PortionSizeGrams { get; set; }

    public DishCategory Category { get; set; }

    /// <summary>Может быть пустым набором (None).</summary>
    public DietaryFlags AdditionalFlags { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public ICollection<DishProduct> Ingredients { get; set; } = new List<DishProduct>();
}
