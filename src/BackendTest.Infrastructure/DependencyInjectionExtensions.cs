using BackendTest.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BackendTest.Infrastructure;

public static class DependencyInjectionExtensions
{
	internal const string PersistenceSectionName = "Sqlite";

	public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
	{
		services.TryAddTransient<AuditSaveChangesInterceptor>();

		services.AddPooledDbContextFactory<BackendTestDbContext>(
			static (provider, contextOptions) =>
			{
				var connString = provider.GetRequiredService<IConfiguration>()
					.GetConnectionString(PersistenceSectionName);

				if (string.IsNullOrWhiteSpace(connString))
				{
					throw new ArgumentNullException(nameof(connString), "Connection string is not set for the database.");
				}

				contextOptions.UseSqlite(
					connString,
					static options =>
					{
						options.MigrationsHistoryTable("ef_migration_history");
					});

				contextOptions.AddInterceptors(provider.GetRequiredService<AuditSaveChangesInterceptor>());

				// Fail fast if any query loads multiple collection navigations
				// without explicit AsSplitQuery() or AsSingleQuery().
				contextOptions.ConfigureWarnings(
					static w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
			});

		return services;
	}

	public static async ValueTask MigrateDatabase(this IServiceProvider serviceProvider)
	{
		await using var scope = serviceProvider.CreateAsyncScope();

		var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BackendTestDbContext>>();
		await using var context = await contextFactory.CreateDbContextAsync();

		await context.Database.MigrateAsync();
	}
}
