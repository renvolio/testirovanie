using Backend.Api.Services.Dishes;
using Xunit;

namespace Backend.Api.Tests;

public class DishNutritionCalculator_BoundaryTests
{
    // граничные значения
    // граммы: -1, 0, 1, 99.9, 100, 101
    // на 100г: -1, 0, 0.1, 1

    public static IEnumerable<object[]> Grams_data()
    {
        return new List<object[]>
        {
            new object[] { -1.0, -1.0 },
            new object[] { 0.0, 0.0 },
            new object[] { 1.0, 1.0 },
            new object[] { 99.9, 99.9 },
            new object[] { 100.0, 100.0 },
            new object[] { 101.0, 101.0 }
        };
    }

    [Theory]
    [MemberData(nameof(Grams_data))]
    // если 100 ккал на 100г, то ккал = граммы (с округлением до 0.1)
    public void SumPerPortionRaw_grams_boundaries(double grams, double expectedKcal)
    {
        var lines = new List<(double, double, double, double, double)>
        {
            (100, 100, 100, 100, grams)
        };

        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(expectedKcal, kcal);
        Assert.Equal(expectedKcal, p);
        Assert.Equal(expectedKcal, f);
        Assert.Equal(expectedKcal, c);
    }

    public static IEnumerable<object[]> Per100_data()
    {
        return new List<object[]>
        {
            new object[] { -1.0, 50.0, -0.5 },
            new object[] { 0.0, 50.0, 0.0 },
            new object[] { 0.2, 50.0, 0.1 }, // 0.2 * 50 / 100 = 0.1 (ровное число)
            new object[] { 1.0, 50.0, 0.5 }
        };
    }

    [Theory]
    [MemberData(nameof(Per100_data))]
    // границы по значению на 100г, проверяем округление до десятых
    public void SumPerPortionRaw_per100_boundaries(double per100, double grams, double expected)
    {
        var lines = new List<(double, double, double, double, double)>
        {
            (per100, per100, per100, per100, grams)
        };

        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(expected, kcal);
        Assert.Equal(expected, p);
        Assert.Equal(expected, f);
        Assert.Equal(expected, c);
    }
}

