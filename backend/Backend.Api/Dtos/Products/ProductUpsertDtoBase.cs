using System.ComponentModel.DataAnnotations;
using Backend.Api.Models.Enums;

namespace Backend.Api.Dtos.Products;

/// <summary>Общие поля создания/обновления продукта (без системных полей).</summary>
public abstract class ProductUpsertDtoBase : IValidatableObject
{
    [Required(ErrorMessage = "Название обязательно.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>До 5 ссылок или путей к изображениям; может быть пустым.</summary>
    public List<string>? PhotoUrls { get; set; }

    public double? CaloriesPer100g { get; set; }

    public double? ProteinsPer100g { get; set; }

    public double? FatsPer100g { get; set; }

    public double? CarbsPer100g { get; set; }

    [MaxLength(8000)]
    public string? Composition { get; set; }

    [Required]
    public ProductCategory? Category { get; set; }

    [Required]
    public CookingRequirement? CookingRequirement { get; set; }

    /// <summary>По умолчанию — ни один флаг; может быть пустым набором.</summary>
    public DietaryFlags? AdditionalFlags { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var trimmed = Name?.Trim() ?? string.Empty;
        if (trimmed.Length < 2)
        {
            yield return new ValidationResult(
                "Название должно содержать не менее 2 символов.",
                new[] { nameof(Name) });
        }

        var photos = NormalizePhotoUrls(PhotoUrls);
        if (photos.Count > 5)
        {
            yield return new ValidationResult(
                "Не более 5 фотографий.",
                new[] { nameof(PhotoUrls) });
        }

        foreach (var err in ValidateMacro(nameof(CaloriesPer100g), CaloriesPer100g, 0, null))
            yield return err;
        foreach (var err in ValidateMacro(nameof(ProteinsPer100g), ProteinsPer100g, 0, 100))
            yield return err;
        foreach (var err in ValidateMacro(nameof(FatsPer100g), FatsPer100g, 0, 100))
            yield return err;
        foreach (var err in ValidateMacro(nameof(CarbsPer100g), CarbsPer100g, 0, 100))
            yield return err;

        if (ProteinsPer100g is not null && FatsPer100g is not null && CarbsPer100g is not null)
        {
            var sumBju = ProteinsPer100g.Value + FatsPer100g.Value + CarbsPer100g.Value;
            if (sumBju > 100.0000001)
            {
                yield return new ValidationResult(
                    "Сумма белков, жиров и углеводов на 100 г не может превышать 100 г.",
                    new[] { nameof(ProteinsPer100g), nameof(FatsPer100g), nameof(CarbsPer100g) });
            }
        }
    }

    internal static List<string> NormalizePhotoUrls(List<string>? photoUrls)
    {
        if (photoUrls is null || photoUrls.Count == 0)
            return new List<string>();

        return photoUrls
            .Select(s => s?.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Cast<string>()
            .ToList();
    }

    private static IEnumerable<ValidationResult> ValidateMacro(
        string field,
        double? value,
        double min,
        double? max)
    {
        if (value is null)
        {
            yield return new ValidationResult("Поле обязательно.", new[] { field });
            yield break;
        }

        if (value < min || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
        {
            yield return new ValidationResult($"Значение должно быть не меньше {min}.", new[] { field });
            yield break;
        }

        if (max is not null && value > max)
        {
            yield return new ValidationResult($"Значение должно быть не больше {max}.", new[] { field });
        }
    }
}
