using Backend.Api.Services.Dishes;
using Xunit;

namespace Backend.Api.Tests;

public class DishNutritionCalculator_EquivalentTests
{
    // эквивалентные классы
    // 1) пустой состав
    // 2) одна строка (обычные положительные значения)
    // 3) несколько строк (сумма)
    // 4) нули на 100г
    // 5) отрицательные значения (как класс)

    [Fact]
    // пустой состав -> всё 0
    public void SumPerPortionRaw_empty_zero()
    {
        var lines = new List<(double, double, double, double, double)>();
        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(0, kcal);
        Assert.Equal(0, p);
        Assert.Equal(0, f);
        Assert.Equal(0, c);
    }

    [Fact]
    // одна строка, проверяем формулу сразу на кбжу
    public void SumPerPortionRaw_oneLine_ok()
    {
        // 200/10/20/30 на 100г, берем 50г => делим пополам
        var lines = new List<(double, double, double, double, double)>
        {
            (200, 10, 20, 30, 50)
        };

        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(100.0, kcal);
        Assert.Equal(5.0, p);
        Assert.Equal(10.0, f);
        Assert.Equal(15.0, c);
    }

    [Fact]
    // несколько строк -> должно суммироваться
    public void SumPerPortionRaw_manyLines_sum()
    {
        // строка1: 100 ккал/100г, 200г => 200 ккал
        // строка2: 50 ккал/100г, 100г => 50 ккал
        var lines = new List<(double, double, double, double, double)>
        {
            (100, 0, 0, 0, 200),
            (50, 0, 0, 0, 100)
        };

        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(250.0, kcal);
        Assert.Equal(0.0, p);
        Assert.Equal(0.0, f);
        Assert.Equal(0.0, c);
    }

    [Fact]
    // нули на 100г -> независимо от грамм всё 0
    public void SumPerPortionRaw_zeroPer100_alwaysZero()
    {
        var lines = new List<(double, double, double, double, double)>
        {
            (0, 0, 0, 0, 9999)
        };

        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(0.0, kcal);
        Assert.Equal(0.0, p);
        Assert.Equal(0.0, f);
        Assert.Equal(0.0, c);
    }

    [Fact]
    // эквивалентный класс отрицательных значений
    // тут калькулятор сам не валидирует, поэтому ждём "минус" в результате
    public void SumPerPortionRaw_negativeValues_keepsSign()
    {
        var lines = new List<(double, double, double, double, double)>
        {
            (-100, -10, -20, -30, 100) // 1 к 1
        };

        var (kcal, p, f, c) = DishNutritionCalculator.SumPerPortionRaw(lines);

        Assert.Equal(-100.0, kcal);
        Assert.Equal(-10.0, p);
        Assert.Equal(-20.0, f);
        Assert.Equal(-30.0, c);
    }
}

