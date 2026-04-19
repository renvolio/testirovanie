using System.ComponentModel.DataAnnotations;
using Backend.Api.Models.Enums;

namespace Backend.Api.Dtos.Dishes;

/// <summary>Общие поля создания/обновления блюда (без системных полей).</summary>
public abstract class DishUpsertDtoBase : IValidatableObject
{
    [Required(ErrorMessage = "Название обязательно.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>До 5 ссылок или путей к изображениям; может быть пустым.</summary>
    public List<string>? PhotoUrls { get; set; }

    /// <summary>Ккал на порцию. Если не передано — подставляется автоматический расчёт по составу.</summary>
    public double? CaloriesPerPortion { get; set; }

    /// <summary>Белки на порцию (г). Если не передано — подставляется автоматический расчёт по составу.</summary>
    public double? ProteinsPerPortion { get; set; }

    /// <summary>Жиры на порцию (г). Если не передано — подставляется автоматический расчёт по составу.</summary>
    public double? FatsPerPortion { get; set; }

    /// <summary>Углеводы на порцию (г). Если не передано — подставляется автоматический расчёт по составу.</summary>
    public double? CarbsPerPortion { get; set; }

    /// <summary>Не менее одной строки: продукт и количество в порции (г).</summary>
    [Required]
    public List<DishCompositionItemDto>? Composition { get; set; }

    public double? PortionSizeGrams { get; set; }

    /// <summary>
    /// Категория из формы. Если не указана — берётся из первого макроса в названии (!десерт, !первое, …).
    /// Если указана и в названии есть макрос — используется значение из поля формы.
    /// </summary>
    public DishCategory? Category { get; set; }

    public DietaryFlags? AdditionalFlags { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Название обязательно.", new[] { nameof(Name) });
        }

        var photos = DishUpsertDtoBase.NormalizePhotoUrls(PhotoUrls);
        if (photos.Count > 5)
        {
            yield return new ValidationResult(
                "Не более 5 фотографий.",
                new[] { nameof(PhotoUrls) });
        }

        foreach (var err in ValidateOptionalNonNegative(nameof(CaloriesPerPortion), CaloriesPerPortion))
            yield return err;
        foreach (var err in ValidateOptionalNonNegative(nameof(ProteinsPerPortion), ProteinsPerPortion))
            yield return err;
        foreach (var err in ValidateOptionalNonNegative(nameof(FatsPerPortion), FatsPerPortion))
            yield return err;
        foreach (var err in ValidateOptionalNonNegative(nameof(CarbsPerPortion), CarbsPerPortion))
            yield return err;

        if (PortionSizeGrams is null)
        {
            yield return new ValidationResult("Размер порции обязателен.", new[] { nameof(PortionSizeGrams) });
        }
        else if (PortionSizeGrams <= 0 || double.IsNaN(PortionSizeGrams.Value) || double.IsInfinity(PortionSizeGrams.Value))
        {
            yield return new ValidationResult(
                "Размер порции должен быть больше 0 г.",
                new[] { nameof(PortionSizeGrams) });
        }

        if (Composition is null || Composition.Count == 0)
        {
            yield return new ValidationResult(
                "Состав должен содержать не менее одного продукта.",
                new[] { nameof(Composition) });
            yield break;
        }

        var seen = new HashSet<Guid>();
        foreach (var line in Composition)
        {
            if (line.ProductId is null || line.ProductId == Guid.Empty)
            {
                yield return new ValidationResult("Укажите продукт.", new[] { nameof(Composition) });
            }
            else if (!seen.Add(line.ProductId.Value))
            {
                yield return new ValidationResult(
                    "Один и тот же продукт не может входить в состав дважды.",
                    new[] { nameof(Composition) });
            }

            if (line.QuantityGrams is null)
            {
                yield return new ValidationResult("Укажите количество продукта (г).", new[] { nameof(Composition) });
            }
            else if (line.QuantityGrams <= 0 || double.IsNaN(line.QuantityGrams.Value) || double.IsInfinity(line.QuantityGrams.Value))
            {
                yield return new ValidationResult(
                    "Количество продукта должно быть больше 0 г.",
                    new[] { nameof(Composition) });
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

    internal static IReadOnlyList<(Guid ProductId, double QuantityGrams)> NormalizeComposition(
        List<DishCompositionItemDto>? composition)
    {
        if (composition is null || composition.Count == 0)
            return Array.Empty<(Guid, double)>();

        return composition
            .Where(l => l.ProductId is not null && l.ProductId != Guid.Empty && l.QuantityGrams is > 0)
            .Select(l => (l.ProductId!.Value, l.QuantityGrams!.Value))
            .ToList();
    }

    /// <summary>Null — будет использован черновик из расчёта; если значение задано — проверяем &gt;= 0.</summary>
    private static IEnumerable<ValidationResult> ValidateOptionalNonNegative(string field, double? value)
    {
        if (value is null)
            yield break;

        if (value < 0 || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
            yield return new ValidationResult("Значение должно быть не меньше 0.", new[] { field });
    }
}
