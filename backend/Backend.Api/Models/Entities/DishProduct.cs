namespace Backend.Api.Models.Entities;

/// <summary>Связь блюдо — продукт с количеством продукта в одной порции блюда (г).</summary>
public class DishProduct
{
    public Guid DishId { get; set; }

    public Dish Dish { get; set; } = null!;

    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    /// <summary>Масса данного продукта в одной порции блюда, г.</summary>
    public double QuantityGrams { get; set; }
}
