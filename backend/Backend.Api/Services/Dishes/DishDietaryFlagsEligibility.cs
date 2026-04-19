using Backend.Api.Models.Entities;
using Backend.Api.Models.Enums;

namespace Backend.Api.Services.Dishes;

/// <summary>
/// Доступность флагов блюда по флагам продуктов в составе (2.4).
/// </summary>
internal static class DishDietaryFlagsEligibility
{
    public static DietaryFlags ComputeAllowedFlags(IReadOnlyList<Product> productsInComposition)
    {
        if (productsInComposition.Count == 0)
            return DietaryFlags.None;

        DietaryFlags allowed = DietaryFlags.None;

        if (productsInComposition.All(p => (p.AdditionalFlags & DietaryFlags.Vegan) == DietaryFlags.Vegan))
            allowed |= DietaryFlags.Vegan;

        if (productsInComposition.All(p => (p.AdditionalFlags & DietaryFlags.GlutenFree) == DietaryFlags.GlutenFree))
            allowed |= DietaryFlags.GlutenFree;

        if (productsInComposition.All(p => (p.AdditionalFlags & DietaryFlags.SugarFree) == DietaryFlags.SugarFree))
            allowed |= DietaryFlags.SugarFree;

        return allowed;
    }

    /// <summary>Снимает с блюда флаги, которые не разрешены составом.</summary>
    public static DietaryFlags SanitizeFlags(DietaryFlags requested, DietaryFlags allowed) =>
        requested & allowed;
}
