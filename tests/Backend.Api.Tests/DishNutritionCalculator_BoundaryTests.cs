using Backend.Api.Services.Dishes;
using Xunit;

namespace Backend.Api.Tests;

public class DishNutritionCalculator_BoundaryTests
{
    // Данные для успешных тестов
    public static IEnumerable<object[]> ValidGrams_Data() => new List<object[]>
    {
        new object[] { 0.0 },
        new object[] { 0.1 }
    };

    // Данные для тестов с ошибкой
    public static IEnumerable<object[]> InvalidGrams_Data() => new List<object[]>
    {
        new object[] { -0.1 }
    };

    [Theory]
    [MemberData(nameof(ValidGrams_Data))]
    public void SumPerPortionRaw_ValidGrams_ReturnsCorrectValues(double grams)
    {
        var lines = new List<(double, double, double, double, double)> { (100, 100, 100, 100, grams) };

        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(grams, kcal);
        Assert.Equal(grams, p);
        Assert.Equal(grams, f);
        Assert.Equal(grams, c);
    }

    [Theory]
    [MemberData(nameof(InvalidGrams_Data))]
    public void SumPerPortionRaw_InvalidGrams_ThrowsException(double grams)
    {
        var lines = new List<(double, double, double, double, double)> { (100, 100, 100, 100, grams) };

        Assert.ThrowsAny<Exception>(() => DishNutritionCalculator.SumPerPortionRaw(lines));
    }
}
