namespace Backend.Api.Models.Enums;

/// <summary>Дополнительные диетические характеристики (продукты и блюда).</summary>
[Flags]
public enum DietaryFlags
{
    None = 0,
    /// <summary>Веган</summary>
    Vegan = 1,
    /// <summary>Без глютена</summary>
    GlutenFree = 2,
    /// <summary>Без сахара</summary>
    SugarFree = 4
}
