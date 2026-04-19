using System.Text.Json;
using Backend.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Api.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(500);

        var photoComparer = new ValueComparer<List<string>>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, StringComparer.Ordinal.GetHashCode(s))),
            v => v.ToList());

        ValueConverter<List<string>, string> photosConverter = new(
            v => JsonSerializer.Serialize(v, JsonOptions),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(v, JsonOptions) ?? new List<string>());

        builder.Property(p => p.PhotoUrls)
            .HasConversion(photosConverter)
            .HasColumnName("PhotoUrlsJson")
            .Metadata.SetValueComparer(photoComparer);

        builder.Property(p => p.Composition)
            .HasMaxLength(8000);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.CookingRequirement)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.AdditionalFlags)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.ModifiedAt);
    }
}
