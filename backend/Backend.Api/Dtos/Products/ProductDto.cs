using Backend.Api.Models.Enums;

namespace Backend.Api.Dtos.Products;

/// <summary>Ответ API: продукт со всеми полями, включая системные даты.</summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<string> PhotoUrls { get; set; } = Array.Empty<string>();
    public double CaloriesPer100g { get; set; }
    public double ProteinsPer100g { get; set; }
    public double FatsPer100g { get; set; }
    public double CarbsPer100g { get; set; }
    public string? Composition { get; set; }
    public ProductCategory Category { get; set; }
    public CookingRequirement CookingRequirement { get; set; }
    public DietaryFlags AdditionalFlags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
