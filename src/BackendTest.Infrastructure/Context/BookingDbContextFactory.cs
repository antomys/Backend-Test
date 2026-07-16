using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendTest.Infrastructure.Context;

/// <inheritdoc />
public sealed class BackendTestDbContextFactory : IDesignTimeDbContextFactory<BackendTestDbContext>
{
	/// <inheritdoc/>
	public BackendTestDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<BackendTestDbContext>().UseSqlite(
			Environment.GetEnvironmentVariable(
				$"ConnectionStrings__{DependencyInjectionExtensions.PersistenceSectionName}"));

		return new BackendTestDbContext(optionsBuilder.Options);
	}
}
