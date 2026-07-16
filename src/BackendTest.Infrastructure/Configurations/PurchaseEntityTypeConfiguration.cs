using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendTest.Infrastructure.Configurations;

/// <inheritdoc />
public sealed class PurchaseEntityTypeConfiguration : IEntityTypeConfiguration<PurchasePersistenceModel>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<PurchasePersistenceModel> builder)
	{
		builder.ToTable("purchases");

		builder.HasKey(static model => model.Id);

		builder.Property(static model => model.CreatedAt);
		builder.Property(static model => model.UpdatedAt);

		builder.HasOne(static model => model.Customer)
			.WithMany()
			.HasForeignKey(static model => model.CustomerId);

		builder.HasMany(static model => model.Products)
			.WithMany()
			.UsingEntity(static join => join.ToTable("purchase_products"));
	}
}
