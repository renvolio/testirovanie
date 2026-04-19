using Backend.Api.Models.Enums;

namespace Backend.Api.Dtos.Dishes;

/// <summary>Параметры списка блюд: фильтры и поиск по названию (2.5).</summary>
public class DishListQuery
{
    public DishCategory? Category { get; set; }

    /// <summary>Только блюда с флагом «веган».</summary>
    public bool? Vegan { get; set; }

    /// <summary>Только блюда с флагом «без глютена».</summary>
    public bool? GlutenFree { get; set; }

    /// <summary>Только блюда с флагом «без сахара».</summary>
    public bool? SugarFree { get; set; }

    /// <summary>Подстрока в названии (без учёта регистра).</summary>
    public string? Search { get; set; }
}
