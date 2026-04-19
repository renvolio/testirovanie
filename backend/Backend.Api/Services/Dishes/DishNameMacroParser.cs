using System.Text.RegularExpressions;
using Backend.Api.Models.Enums;

namespace Backend.Api.Services.Dishes;

/// <summary>
/// Макросы категории в названии блюда (!десерт, !первое, …). Учитывается только первое вхождение в строке слева направо.
/// </summary>
internal static class DishNameMacroParser
{
    private static readonly (string Token, DishCategory Category)[] Macros =
    [
        ("!перекус", DishCategory.Snack),
        ("!первое", DishCategory.FirstCourse),
        ("!второе", DishCategory.SecondCourse),
        ("!десерт", DishCategory.Dessert),
        ("!напиток", DishCategory.Drink),
        ("!салат", DishCategory.Salad),
        ("!суп", DishCategory.Soup)
    ];

    /// <summary>
    /// Находит самое раннее вхождение любого макроса, возвращает категорию с этого макроса и имя без этого вхождения.
    /// </summary>
    public static (string CleanedName, DishCategory? CategoryFromMacro) StripFirstCategoryMacro(string name)
    {
        if (string.IsNullOrEmpty(name))
            return (name, null);

        var bestIndex = int.MaxValue;
        string? matchedToken = null;
        DishCategory? matchedCategory = null;

        foreach (var (token, category) in Macros)
        {
            var idx = 0;
            while (true)
            {
                idx = name.IndexOf(token, idx, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                    break;

                if (idx < bestIndex)
                {
                    bestIndex = idx;
                    matchedToken = token;
                    matchedCategory = category;
                }

                idx++;
            }
        }

        if (bestIndex == int.MaxValue || matchedToken is null)
            return (NormalizeWhitespace(name), null);

        var before = bestIndex > 0 ? name[..bestIndex] : string.Empty;
        var afterStart = bestIndex + matchedToken.Length;
        var after = afterStart < name.Length ? name[afterStart..] : string.Empty;
        var merged = string.Concat(before, after);
        return (NormalizeWhitespace(merged), matchedCategory);
    }

    private static string NormalizeWhitespace(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var s = Regex.Replace(value, @"\s+", " ", RegexOptions.CultureInvariant);
        return s.Trim();
    }
}
