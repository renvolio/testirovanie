using Backend.Api.Models.Entities;

namespace Backend.Api.Services.Dishes;

internal static class DishNutritionCalculator
{
    public static (double Calories, double Proteins, double Fats, double Carbs) SumPerPortion(
        IEnumerable<(Product Product, double QuantityGrams)> lines)
    {
        var raw = lines.Select(l => (
            l.Product.CaloriesPer100g,
            l.Product.ProteinsPer100g,
            l.Product.FatsPer100g,
            l.Product.CarbsPer100g,
            l.QuantityGrams
        ));

        return SumPerPortionRaw(raw);
    }

    // заглушка для тестов: чтобы не собирать Product
    // формат: (ккал/100г, белки/100г, жиры/100г, углеводы/100г, граммы)
    public static (double Calories, double Proteins, double Fats, double Carbs) SumPerPortionRaw(
        IEnumerable<(double CaloriesPer100g, double ProteinsPer100g, double FatsPer100g, double CarbsPer100g, double QuantityGrams)> lines)
    {
        double calories = 0, proteins = 0, fats = 0, carbs = 0;

        foreach (var (cal100, p100, f100, c100, grams) in lines)
        {
                if (grams < 0 || cal100 < 0 || p100 < 0 || f100 < 0 || c100 < 0)
                    throw new ArgumentException("Negative values are not allowed");
            var factor = grams / 100.0;
            calories += cal100 * factor;
            proteins += p100 * factor;
            fats += f100 * factor;
            carbs += c100 * factor;
        }

        // округление до десятых
        return (
            Math.Round(calories, 1),
            Math.Round(proteins, 1),
            Math.Round(fats, 1),
            Math.Round(carbs, 1)
        );
    }
}
