using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BackendTest.Infrastructure.Configurations;

/// <inheritdoc />
public sealed class PersonEntityTypeConfiguration : IEntityTypeConfiguration<PersonPersistenceModel>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<PersonPersistenceModel> builder)
	{
		builder.ToTable("persons");

		builder.HasKey(static model => model.Id);

		builder.Property(static model => model.FirstName);
		builder.Property(static model => model.LastName);
		builder.Property(static model => model.YearOfBirth);

		builder.Property(static model => model.CreatedAt);
		builder.Property(static model => model.UpdatedAt);
	}
}
