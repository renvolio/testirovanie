using Backend.Api.Data;
using Backend.Api.Dtos.Products;
using Backend.Api.Models.Entities;
using Backend.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProductDto>> QueryAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default)
    {
        var q = _db.Products.AsNoTracking();

        if (query.Category is not null)
            q = q.Where(p => p.Category == query.Category);

        if (query.CookingRequirement is not null)
            q = q.Where(p => p.CookingRequirement == query.CookingRequirement);

        if (query.Vegan == true)
            q = q.Where(p => (p.AdditionalFlags & DietaryFlags.Vegan) == DietaryFlags.Vegan);

        if (query.GlutenFree == true)
            q = q.Where(p => (p.AdditionalFlags & DietaryFlags.GlutenFree) == DietaryFlags.GlutenFree);

        if (query.SugarFree == true)
            q = q.Where(p => (p.AdditionalFlags & DietaryFlags.SugarFree) == DietaryFlags.SugarFree);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            q = q.Where(p => p.Name.ToLower().Contains(term));
        }

        var sortKey = query.SortBy?.Trim().ToLowerInvariant() ?? "name";
        var desc = query.SortDescending;

        q = sortKey switch
        {
            "calories" => desc
                ? q.OrderByDescending(p => p.CaloriesPer100g).ThenBy(p => p.Name)
                : q.OrderBy(p => p.CaloriesPer100g).ThenBy(p => p.Name),
            "proteins" => desc
                ? q.OrderByDescending(p => p.ProteinsPer100g).ThenBy(p => p.Name)
                : q.OrderBy(p => p.ProteinsPer100g).ThenBy(p => p.Name),
            "fats" => desc
                ? q.OrderByDescending(p => p.FatsPer100g).ThenBy(p => p.Name)
                : q.OrderBy(p => p.FatsPer100g).ThenBy(p => p.Name),
            "carbs" => desc
                ? q.OrderByDescending(p => p.CarbsPer100g).ThenBy(p => p.Name)
                : q.OrderBy(p => p.CarbsPer100g).ThenBy(p => p.Name),
            _ => desc
                ? q.OrderByDescending(p => p.Name)
                : q.OrderBy(p => p.Name)
        };

        var items = await q.ToListAsync(cancellationToken);
        return items.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var photos = ProductUpsertDtoBase.NormalizePhotoUrls(dto.PhotoUrls);

        var entity = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            PhotoUrls = photos,
            CaloriesPer100g = dto.CaloriesPer100g!.Value,
            ProteinsPer100g = dto.ProteinsPer100g!.Value,
            FatsPer100g = dto.FatsPer100g!.Value,
            CarbsPer100g = dto.CarbsPer100g!.Value,
            Composition = string.IsNullOrWhiteSpace(dto.Composition) ? null : dto.Composition.Trim(),
            Category = dto.Category!.Value,
            CookingRequirement = dto.CookingRequirement!.Value,
            AdditionalFlags = dto.AdditionalFlags ?? DietaryFlags.None,
            CreatedAt = now,
            ModifiedAt = null
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null)
            return null;

        var photos = ProductUpsertDtoBase.NormalizePhotoUrls(dto.PhotoUrls);

        entity.Name = dto.Name.Trim();
        entity.PhotoUrls = photos;
        entity.CaloriesPer100g = dto.CaloriesPer100g!.Value;
        entity.ProteinsPer100g = dto.ProteinsPer100g!.Value;
        entity.FatsPer100g = dto.FatsPer100g!.Value;
        entity.CarbsPer100g = dto.CarbsPer100g!.Value;
        entity.Composition = string.IsNullOrWhiteSpace(dto.Composition) ? null : dto.Composition.Trim();
        entity.Category = dto.Category!.Value;
        entity.CookingRequirement = dto.CookingRequirement!.Value;
        entity.AdditionalFlags = dto.AdditionalFlags ?? DietaryFlags.None;
        entity.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<ProductDeleteResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (entity is null)
            return new ProductDeleteResult(ProductDeleteStatus.NotFound, null);

        var blockingRows = await _db.Dishes
            .AsNoTracking()
            .Where(d => d.Ingredients.Any(i => i.ProductId == id))
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name })
            .ToListAsync(cancellationToken);

        if (blockingRows.Count > 0)
        {
            var blocking = blockingRows.Select(x => (x.Id, x.Name)).ToList();
            return new ProductDeleteResult(ProductDeleteStatus.BlockedByDishes, blocking);
        }

        _db.Products.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new ProductDeleteResult(ProductDeleteStatus.Deleted, null);
    }

    private static ProductDto MapToDto(Product entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        PhotoUrls = entity.PhotoUrls.ToList(),
        CaloriesPer100g = entity.CaloriesPer100g,
        ProteinsPer100g = entity.ProteinsPer100g,
        FatsPer100g = entity.FatsPer100g,
        CarbsPer100g = entity.CarbsPer100g,
        Composition = entity.Composition,
        Category = entity.Category,
        CookingRequirement = entity.CookingRequirement,
        AdditionalFlags = entity.AdditionalFlags,
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt
    };
}
