using BackendTest.Host;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BackendTest.Integration.Tests.Fixtures;

/// <summary>
/// Boots the Host app against a private, temp-file SQLite database per test class, so
/// Program's existing startup migration/seed runs in isolation for each fixture.
/// </summary>
public sealed class BackendTestWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"backendtest-{Guid.NewGuid():N}.db");

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Development");

		builder.ConfigureAppConfiguration((_, config) =>
		{
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["ConnectionStrings:Sqlite"] = $"Data Source={_databasePath}",
				["Validation:Person:MinimumAge"] = "18",
			});
		});
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		File.Delete(_databasePath);
		File.Delete($"{_databasePath}-wal");
		File.Delete($"{_databasePath}-shm");
	}
}
