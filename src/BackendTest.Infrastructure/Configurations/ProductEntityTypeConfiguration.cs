using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendTest.Infrastructure.Configurations;

public sealed class ProductEntityTypeConfiguration : IEntityTypeConfiguration<ProductPersistenceModel>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<ProductPersistenceModel> builder)
	{
		builder.ToTable("products");

		builder.HasKey(static model => model.Id);

		builder.Property(static model => model.Name);
		builder.Property(static model => model.Type);
		builder.Property(static model => model.Price);

		builder.Property(static model => model.CreatedAt);
		builder.Property(static model => model.UpdatedAt);
	}
}
