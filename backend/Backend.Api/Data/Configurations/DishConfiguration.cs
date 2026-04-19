using System.Text.Json;
using Backend.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Api.Data.Configurations;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        builder.ToTable("dishes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
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

        builder.Property(d => d.PhotoUrls)
            .HasConversion(photosConverter)
            .HasColumnName("PhotoUrlsJson")
            .Metadata.SetValueComparer(photoComparer);

        builder.Property(d => d.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.AdditionalFlags)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.ModifiedAt);
    }
}
