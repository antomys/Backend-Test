using BackendTest.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendTest.Infrastructure.Context;

/// <summary>
/// The primary database context for the NBA Service, managing persistence for activities, metrics, and logs.
/// </summary>
public sealed class BackendTestDbContext(DbContextOptions<BackendTestDbContext> options) : DbContext(options)
{
	public DbSet<PersonPersistenceModel> Persons { get; init; }

	public DbSet<ProductPersistenceModel> Products { get; init; }

	public DbSet<PurchasePersistenceModel> Purchases { get; init; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(BackendTestDbContext).Assembly);

		base.OnModelCreating(modelBuilder);
	}
}
