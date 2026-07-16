using System.Net.Mime;
using System.Text;
using System.Text.Json;
using BackendTest.Host;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace BackendTest.Host.Extensions;

public static class HealthCheckExtensions
{
	private const string StartupRoute = "/health";
	private const string ReadinessRoute = "/health/readiness";
	private const string LivenessRoute = "/health/liveness";
	private const string StatusTag = "status";
	private const string CriticalTag = "critical";

	public static WebApplication UseHealthChecks(this WebApplication app)
	{
		app.UseHealthChecks(StartupRoute);

		app.UseHealthChecks(LivenessRoute);

		app.UseHealthChecks(
			ReadinessRoute,
			new HealthCheckOptions
			{
				Predicate = check => check.Tags.Contains(CriticalTag),
				ResponseWriter = static async (context, report) =>
				{
					context.Response.ContentType = MediaTypeNames.Text.Plain;
					await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(Enum.GetName(report.Status) ?? string.Empty));
				},
			});

		app.UseHealthChecks(
			$"{StartupRoute}/{StatusTag}",
			new HealthCheckOptions
			{
				ResponseWriter = static async (context, report) =>
				{
					context.Response.ContentType = MediaTypeNames.Application.Json;
					if (report is not null)
					{
						await JsonSerializer.SerializeAsync(context.Response.Body, report, ExceptionJsonConverter.WithSafeException);

						return;
					}

					await context.Response.BodyWriter.WriteAsync(ReadOnlyMemory<byte>.Empty);
				},
				Predicate = _ => true,
			});

		return app;
	}
}
