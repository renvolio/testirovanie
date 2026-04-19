using Backend.Api.Models.Enums;

namespace Backend.Api.Dtos.Products;

/// <summary>Параметры списка продуктов: фильтры, поиск, сортировка.</summary>
public class ProductListQuery
{
    public ProductCategory? Category { get; set; }

    public CookingRequirement? CookingRequirement { get; set; }

    /// <summary>Требуется флаг «веган».</summary>
    public bool? Vegan { get; set; }

    /// <summary>Требуется флаг «без глютена».</summary>
    public bool? GlutenFree { get; set; }

    /// <summary>Требуется флаг «без сахара».</summary>
    public bool? SugarFree { get; set; }

    /// <summary>Подстрока в названии (без учёта регистра).</summary>
    public string? Search { get; set; }

    /// <summary>name | calories | proteins | fats | carbs (без учёта регистра).</summary>
    public string? SortBy { get; set; }

    /// <summary>По убыванию; по умолчанию по возрастанию.</summary>
    public bool SortDescending { get; set; }
}
