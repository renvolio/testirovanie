namespace Backend.Api.Dtos.Dishes;

public class DishCompositionItemResponseDto
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public double QuantityGrams { get; set; }
}
