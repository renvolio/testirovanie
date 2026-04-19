using Backend.Api.Models.Enums;

namespace Backend.Api.Models.Entities;

/// <summary>Продукт (сущность для хранения в БД).</summary>
public class Product
{
    public Guid Id { get; set; }

    /// <summary>Наименование продукта, минимум 2 символа.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ссылки или пути к изображениям (0–5 штук).</summary>
    public List<string> PhotoUrls { get; set; } = new();

    /// <summary>Ккал на 100 г, не меньше 0.</summary>
    public double CaloriesPer100g { get; set; }

    /// <summary>Белки, г на 100 г, 0–100.</summary>
    public double ProteinsPer100g { get; set; }

    /// <summary>Жиры, г на 100 г, 0–100.</summary>
    public double FatsPer100g { get; set; }

    /// <summary>Углеводы, г на 100 г, 0–100.</summary>
    public double CarbsPer100g { get; set; }

    /// <summary>Текстовое описание состава.</summary>
    public string? Composition { get; set; }

    public ProductCategory Category { get; set; }

    public CookingRequirement CookingRequirement { get; set; }

    /// <summary>Может быть пустым набором (None).</summary>
    public DietaryFlags AdditionalFlags { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public ICollection<DishProduct> DishProducts { get; set; } = new List<DishProduct>();
}
