namespace Backend.Api.Models.Enums;

/// <summary>Степень готовности продукта к употреблению.</summary>
public enum CookingRequirement
{
    /// <summary>Готовый к употреблению</summary>
    ReadyToEat = 0,
    /// <summary>Полуфабрикат</summary>
    SemiFinished = 1,
    /// <summary>Требует приготовления</summary>
    RequiresCooking = 2
}
