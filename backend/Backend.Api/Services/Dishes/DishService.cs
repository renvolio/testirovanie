using Backend.Api.Data;
using Backend.Api.Dtos.Dishes;
using Backend.Api.Models.Entities;
using Backend.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Dishes;

public class DishService : IDishService
{
    private readonly AppDbContext _db;

    public DishService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DishDto>> QueryAsync(
        DishListQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Dishes
            .AsNoTracking()
            .Include(d => d.Ingredients)
            .ThenInclude(i => i.Product);

        if (query.Category is not null)
            q = q.Where(d => d.Category == query.Category);

        if (query.Vegan == true)
            q = q.Where(d => (d.AdditionalFlags & DietaryFlags.Vegan) == DietaryFlags.Vegan);

        if (query.GlutenFree == true)
            q = q.Where(d => (d.AdditionalFlags & DietaryFlags.GlutenFree) == DietaryFlags.GlutenFree);

        if (query.SugarFree == true)
            q = q.Where(d => (d.AdditionalFlags & DietaryFlags.SugarFree) == DietaryFlags.SugarFree);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            q = q.Where(d => d.Name.ToLower().Contains(term));
        }

        var items = await q.OrderBy(d => d.Name).ToListAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<DishDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Dishes
            .AsNoTracking()
            .Include(d => d.Ingredients)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<DishDto> CreateAsync(CreateDishDto dto, CancellationToken cancellationToken = default)
    {
        var lines = DishUpsertDtoBase.NormalizeComposition(dto.Composition);
        var productById = await LoadProductsForCompositionAsync(lines, cancellationToken);
        var (calcCal, calcProt, calcFat, calcCarb) = ComputeNutritionFromComposition(lines, productById);

        var now = DateTime.UtcNow;
        var photos = DishUpsertDtoBase.NormalizePhotoUrls(dto.PhotoUrls);

        var trimmedName = dto.Name.Trim();
        var (cleanName, categoryFromMacro) = DishNameMacroParser.StripFirstCategoryMacro(trimmedName);
        var category = dto.Category ?? categoryFromMacro
            ?? throw new ArgumentException(
                "Укажите категорию в поле формы или один из макросов в названии: !десерт, !первое, !второе, !напиток, !салат, !суп, !перекус.");

        if (cleanName.Length < 2)
        {
            throw new ArgumentException(
                "Название после обработки макросов должно содержать не менее 2 символов.");
        }

        var portionGrams = dto.PortionSizeGrams!.Value;
        var proteins = dto.ProteinsPerPortion ?? calcProt;
        var fats = dto.FatsPerPortion ?? calcFat;
        var carbs = dto.CarbsPerPortion ?? calcCarb;
        var calories = dto.CaloriesPerPortion ?? calcCal;

        ValidateDishBjuSumPer100Grams(portionGrams, proteins, fats, carbs);

        var productsInComposition = lines.Select(l => productById[l.ProductId]).ToList();
        var allowedFlags = DishDietaryFlagsEligibility.ComputeAllowedFlags(productsInComposition);
        var requestedFlags = dto.AdditionalFlags ?? DietaryFlags.None;
        var finalFlags = DishDietaryFlagsEligibility.SanitizeFlags(requestedFlags, allowedFlags);

        var dish = new Dish
        {
            Id = Guid.NewGuid(),
            Name = cleanName,
            PhotoUrls = photos,
            CaloriesPerPortion = calories,
            ProteinsPerPortion = proteins,
            FatsPerPortion = fats,
            CarbsPerPortion = carbs,
            PortionSizeGrams = portionGrams,
            Category = category,
            AdditionalFlags = finalFlags,
            CreatedAt = now,
            ModifiedAt = null
        };

        foreach (var (productId, quantityGrams) in lines)
        {
            dish.Ingredients.Add(new DishProduct
            {
                ProductId = productId,
                QuantityGrams = quantityGrams
            });
        }

        _db.Dishes.Add(dish);
        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(dish).Collection(d => d.Ingredients).Query().Include(i => i.Product).LoadAsync(cancellationToken);

        return MapToDto(dish);
    }

    public async Task<DishDto?> UpdateAsync(Guid id, UpdateDishDto dto, CancellationToken cancellationToken = default)
    {
        var dish = await _db.Dishes
            .Include(d => d.Ingredients)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dish is null)
            return null;

        var lines = DishUpsertDtoBase.NormalizeComposition(dto.Composition);
        var productById = await LoadProductsForCompositionAsync(lines, cancellationToken);
        var (calcCal, calcProt, calcFat, calcCarb) = ComputeNutritionFromComposition(lines, productById);

        var photos = DishUpsertDtoBase.NormalizePhotoUrls(dto.PhotoUrls);

        var trimmedName = dto.Name.Trim();
        var (cleanName, categoryFromMacro) = DishNameMacroParser.StripFirstCategoryMacro(trimmedName);
        var category = dto.Category ?? categoryFromMacro
            ?? throw new ArgumentException(
                "Укажите категорию в поле формы или один из макросов в названии: !десерт, !первое, !второе, !напиток, !салат, !суп, !перекус.");

        if (cleanName.Length < 2)
        {
            throw new ArgumentException(
                "Название после обработки макросов должно содержать не менее 2 символов.");
        }

        var portionGrams = dto.PortionSizeGrams!.Value;
        var proteins = dto.ProteinsPerPortion ?? calcProt;
        var fats = dto.FatsPerPortion ?? calcFat;
        var carbs = dto.CarbsPerPortion ?? calcCarb;
        var calories = dto.CaloriesPerPortion ?? calcCal;

        ValidateDishBjuSumPer100Grams(portionGrams, proteins, fats, carbs);

        var productsInComposition = lines.Select(l => productById[l.ProductId]).ToList();
        var allowedFlags = DishDietaryFlagsEligibility.ComputeAllowedFlags(productsInComposition);
        var requestedFlags = dto.AdditionalFlags ?? DietaryFlags.None;
        var finalFlags = DishDietaryFlagsEligibility.SanitizeFlags(requestedFlags, allowedFlags);

        dish.Name = cleanName;
        dish.PhotoUrls = photos;
        dish.CaloriesPerPortion = calories;
        dish.ProteinsPerPortion = proteins;
        dish.FatsPerPortion = fats;
        dish.CarbsPerPortion = carbs;
        dish.PortionSizeGrams = portionGrams;
        dish.Category = category;
        dish.AdditionalFlags = finalFlags;
        dish.ModifiedAt = DateTime.UtcNow;

        dish.Ingredients.Clear();
        foreach (var (productId, quantityGrams) in lines)
        {
            dish.Ingredients.Add(new DishProduct
            {
                ProductId = productId,
                QuantityGrams = quantityGrams
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(dish).Collection(d => d.Ingredients).Query().Include(i => i.Product).LoadAsync(cancellationToken);

        return MapToDto(dish);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Dishes.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (entity is null)
            return false;

        _db.Dishes.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Dictionary<Guid, Product>> LoadProductsForCompositionAsync(
        IReadOnlyList<(Guid ProductId, double QuantityGrams)> lines,
        CancellationToken cancellationToken)
    {
        var ids = lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        if (products.Count != ids.Count)
        {
            var missing = ids.Where(i => !products.ContainsKey(i)).ToArray();
            throw new ArgumentException($"Не найдены продукты: {string.Join(", ", missing)}");
        }

        return products;
    }

    /// <summary>
    /// КБЖУ порции по правилам: для каждого продукта макро_на_100_г × количество_в_порции_г / 100, сумма по составу.
    /// </summary>
    private static (double Calories, double Proteins, double Fats, double Carbs) ComputeNutritionFromComposition(
        IReadOnlyList<(Guid ProductId, double QuantityGrams)> lines,
        IReadOnlyDictionary<Guid, Product> productById)
    {
        var sequence = lines.Select(l => (productById[l.ProductId], l.QuantityGrams));
        return DishNutritionCalculator.SumPerPortion(sequence);
    }

    /// <summary>
    /// Для блюда: (белки+жиры+углеводы на порцию) / масса порции × 100 — не больше 100 г БЖУ на 100 г блюда.
    /// </summary>
    private static void ValidateDishBjuSumPer100Grams(
        double portionGrams,
        double proteins,
        double fats,
        double carbs)
    {
        if (portionGrams <= 0 || double.IsNaN(portionGrams) || double.IsInfinity(portionGrams))
            return;

        var sumBjuPer100 = (proteins + fats + carbs) / portionGrams * 100.0;
        if (sumBjuPer100 > 100.0000001)
        {
            throw new ArgumentException(
                "Сумма белков, жиров и углеводов, пересчитанная на 100 г блюда, не может превышать 100 г.");
        }
    }

    private static DishDto MapToDto(Dish dish)
    {
        var pairs = dish.Ingredients
            .Select(i => (i.Product, i.QuantityGrams))
            .ToList();

        var (sCal, sProt, sFat, sCarb) = DishNutritionCalculator.SumPerPortion(pairs);

        var productsInDish = dish.Ingredients.Select(i => i.Product).ToList();
        var allowedFlags = DishDietaryFlagsEligibility.ComputeAllowedFlags(productsInDish);

        var ingredients = dish.Ingredients
            .OrderBy(i => i.Product.Name)
            .Select(i => new DishCompositionItemResponseDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                QuantityGrams = i.QuantityGrams
            })
            .ToList();

        return new DishDto
        {
            Id = dish.Id,
            Name = dish.Name,
            PhotoUrls = dish.PhotoUrls.ToList(),
            CaloriesPerPortion = dish.CaloriesPerPortion,
            ProteinsPerPortion = dish.ProteinsPerPortion,
            FatsPerPortion = dish.FatsPerPortion,
            CarbsPerPortion = dish.CarbsPerPortion,
            PortionSizeGrams = dish.PortionSizeGrams,
            Category = dish.Category,
            AdditionalFlags = dish.AdditionalFlags,
            AllowedAdditionalFlags = allowedFlags,
            Ingredients = ingredients,
            CreatedAt = dish.CreatedAt,
            ModifiedAt = dish.ModifiedAt,
            SuggestedCaloriesPerPortion = sCal,
            SuggestedProteinsPerPortion = sProt,
            SuggestedFatsPerPortion = sFat,
            SuggestedCarbsPerPortion = sCarb
        };
    }
}
