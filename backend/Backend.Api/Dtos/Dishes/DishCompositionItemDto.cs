namespace Backend.Api.Dtos.Dishes;

/// <summary>Строка состава блюда: продукт и масса в одной порции (г).</summary>
public class DishCompositionItemDto
{
    public Guid? ProductId { get; set; }

    public double? QuantityGrams { get; set; }
}
