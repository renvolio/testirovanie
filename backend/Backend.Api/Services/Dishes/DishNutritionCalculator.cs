using Backend.Api.Models.Entities;

namespace Backend.Api.Services.Dishes;

internal static class DishNutritionCalculator
{
    /// <summary>
    /// Суммирует КБЖУ на одну порцию блюда: для каждой строки состава
    /// значение_на_100_г × количество_продукта_в_порции_г / 100, затем сумма по всем продуктам.
    /// </summary>
    public static (double Calories, double Proteins, double Fats, double Carbs) SumPerPortion(
        IEnumerable<(Product Product, double QuantityGrams)> lines)
    {
        double calories = 0, proteins = 0, fats = 0, carbs = 0;

        foreach (var (product, grams) in lines)
        {
            var factor = grams / 100.0;
            calories += product.CaloriesPer100g * factor;
            proteins += product.ProteinsPer100g * factor;
            fats += product.FatsPer100g * factor;
            carbs += product.CarbsPer100g * factor;
        }

        // по заданию округляем до десятых
        return (
            Math.Round(calories, 1),
            Math.Round(proteins, 1),
            Math.Round(fats, 1),
            Math.Round(carbs, 1)
        );
    }
}
