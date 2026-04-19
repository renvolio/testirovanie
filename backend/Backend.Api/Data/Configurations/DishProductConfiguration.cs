using Backend.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Api.Data.Configurations;

public class DishProductConfiguration : IEntityTypeConfiguration<DishProduct>
{
    public void Configure(EntityTypeBuilder<DishProduct> builder)
    {
        builder.ToTable("dish_products");

        builder.HasKey(dp => new { dp.DishId, dp.ProductId });

        builder.Property(dp => dp.QuantityGrams)
            .IsRequired();

        builder.HasOne(dp => dp.Dish)
            .WithMany(d => d.Ingredients)
            .HasForeignKey(dp => dp.DishId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dp => dp.Product)
            .WithMany(p => p.DishProducts)
            .HasForeignKey(dp => dp.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
