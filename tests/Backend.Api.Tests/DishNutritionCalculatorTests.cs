using Backend.Api.Models.Entities;
using Backend.Api.Services.Dishes;
using Xunit;

namespace Backend.Api.Tests;

// тесты на автоматический расчёт калорийности блюда
//
// тут я сделал два подхода:
// 1- эквивалентное разб.:
//    - пустой состав
//    - один ингредиент
//    - несколько ингредиентов
//    - нулевое значение на 100г
// 2- анализ граничных значений
//    - граммы около 0 и около 100 (0, 1, 99.99, 100, 101)

public class DishNutritionCalculatorTests
{
    private static Product P(
        double caloriesPer100g,
        double proteinsPer100g = 0,
        double fatsPer100g = 0,
        double carbsPer100g = 0)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = "p",
            CaloriesPer100g = caloriesPer100g,
            ProteinsPer100g = proteinsPer100g,
            FatsPer100g = fatsPer100g,
            CarbsPer100g = carbsPer100g
        };
    }

    [Fact]
    // эквивалентный класс "пустой состав"
    // ожидаю нули по всем полям
    public void SumPerPortion_empty_returns_zero()
    {
        var lines = new List<(Product, double)>();
        var (cal, prot, fat, carb) = DishNutritionCalculator.SumPerPortion(lines);

        Assert.Equal(0, cal);
        Assert.Equal(0, prot);
        Assert.Equal(0, fat);
        Assert.Equal(0, carb);
    }

    [Fact]
    // эквивалентный класс "один ингредиент"
    // проверяю простую формулу на 50г
    public void SumPerPortion_one_line_works()
    {
        var product = P(caloriesPer100g: 200, proteinsPer100g: 10, fatsPer100g: 20, carbsPer100g: 30);
        var lines = new List<(Product, double)> { (product, 50.0) };

        var (cal, prot, fat, carb) = DishNutritionCalculator.SumPerPortion(lines);

        Assert.Equal(100, cal, 10);
        Assert.Equal(5, prot, 10);
        Assert.Equal(10, fat, 10);
        Assert.Equal(15, carb, 10);
    }

    [Fact]
    // эквивалентный класс "несколько ингредиентов"
    // должно просто сложиться по всем строкам
    public void SumPerPortion_many_lines_sums()
    {
        var a = P(caloriesPer100g: 100);
        var b = P(caloriesPer100g: 50);

        var lines = new List<(Product, double)>
        {
            (a, 200.0), // 200 ккал
            (b, 100.0)  // 50 ккал
        };

        var (cal, _, _, _) = DishNutritionCalculator.SumPerPortion(lines);

        Assert.Equal(250, cal, 10);
    }

    [Fact]
    // эквивалентный класс "нулевые значения на 100г"
    // при любых граммах результат должен быть 0
    public void SumPerPortion_zero_per100g_gives_zero()
    {
        var product = P(caloriesPer100g: 0, proteinsPer100g: 0, fatsPer100g: 0, carbsPer100g: 0);
        var lines = new List<(Product, double)> { (product, 9999.0) };

        var (cal, prot, fat, carb) = DishNutritionCalculator.SumPerPortion(lines);

        Assert.Equal(0, cal);
        Assert.Equal(0, prot);
        Assert.Equal(0, fat);
        Assert.Equal(0, carb);
    }

    public static IEnumerable<object[]> Grams_boundary_data()
    {
        // анализ граничных значений около 0 и около 100
        yield return new object[] { 0.0, 0.0 };
        yield return new object[] { 1.0, 1.0 };
        yield return new object[] { 99.99, 99.99 };
        yield return new object[] { 100.0, 100.0 };
        yield return new object[] { 101.0, 101.0 };
    }

    [Theory]
    [MemberData(nameof(Grams_boundary_data))]
    /// <summary>
    /// границы по граммам
    /// беру 100 ккал на 100г, тогда результат по ккал = граммы
    /// </summary>
    public void SumPerPortion_grams_boundaries(double grams, double expectedCalories)
    {
        var product = P(caloriesPer100g: 100);
        var lines = new List<(Product, double)> { (product, grams) };

        var (cal, _, _, _) = DishNutritionCalculator.SumPerPortion(lines);

        Assert.Equal(expectedCalories, cal, 8);
    }

    public static IEnumerable<object[]> CaloriesPer100g_boundary_data()
    {
        // границы по значению на 100г, тут хотя бы 0 и около 0
        yield return new object[] { 0.0, 50.0, 0.0 };
        yield return new object[] { 0.01, 50.0, 0.005 };
        yield return new object[] { 1.0, 50.0, 0.5 };
        yield return new object[] { 999.0, 50.0, 499.5 };
    }

    [Theory]
    [MemberData(nameof(CaloriesPer100g_boundary_data))]
    /// <summary>
    /// границы по ккал на 100г
    /// </summary>
    public void SumPerPortion_calories_per100g_boundaries(double caloriesPer100g, double grams, double expectedCalories)
    {
        var product = P(caloriesPer100g: caloriesPer100g);
        var lines = new List<(Product, double)> { (product, grams) };

        var (cal, _, _, _) = DishNutritionCalculator.SumPerPortion(lines);

        Assert.Equal(expectedCalories, cal, 8);
    }
}

