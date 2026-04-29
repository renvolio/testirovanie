using Backend.Api.Services.Dishes;
using Xunit;

namespace Backend.Api.Tests;

public class DishNutritionCalculator_BoundaryTests
{
    public static IEnumerable<object[]> Grams_data()
    {
        return new List<object[]>
        {
            new object[] { -0.1, true },  // ожидаем ошибку
            new object[] { 0.0, false },
            new object[] { 0.1, false }
        };
    }

    [Theory]
    [MemberData(nameof(Grams_data))]
    // проверка веса ингредиента (grams)
    public void SumPerPortionRaw_grams_boundaries(double grams, bool shouldThrow)
    {
        var lines = new List<(double, double, double, double, double)>
        {
            (100, 100, 100, 100, grams)
        };

        if (shouldThrow)
        {
            Assert.ThrowsAny<Exception>(() =>
                DishNutritionCalculator.SumPerPortionRaw(lines));
        }
        else
        {
            var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

            Assert.Equal(grams, kcal);
            Assert.Equal(grams, p);
            Assert.Equal(grams, f);
            Assert.Equal(grams, c);
        }
    }
}