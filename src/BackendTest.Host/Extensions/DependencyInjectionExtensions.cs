using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using Asp.Versioning;
using BackendTest.Host;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Console;

namespace BackendTest.Host.Extensions;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddLogging(
		this IServiceCollection services,
		IHostEnvironment hostEnvironment)
	{
		if (hostEnvironment.IsDevelopment())
		{
			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.ClearProviders();
				loggingBuilder.AddConsole(options => options.FormatterName = ConsoleFormatterNames.Simple);
				loggingBuilder.AddSimpleConsole(options =>
				{
					options.ColorBehavior = LoggerColorBehavior.Enabled;
					options.IncludeScopes = true;
					options.SingleLine = false;
					options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
				});
			});

			return services;
		}

		services.AddLogging(builder =>
		{
			// Configure activity tracking
			builder.Configure(options => options.ActivityTrackingOptions =
				ActivityTrackingOptions.SpanId | ActivityTrackingOptions.TraceId | ActivityTrackingOptions.ParentId);
			builder.AddJsonConsole(options =>
			{
				options.UseUtcTimestamp = true;
				options.TimestampFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffff'Z'";
				options.IncludeScopes = true;
				options.JsonWriterOptions = new JsonWriterOptions
				{
					Indented = hostEnvironment.IsDevelopment(), // Compact output, no indentation for production
					Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				};
			});
		});

		services.AddServiceLogEnricher(options =>
		{
			options.ApplicationName = true;
			options.EnvironmentName = true;
		});

		services.AddApplicationMetadata(options =>
		{
			options.ApplicationName = Assembly.GetExecutingAssembly().FullName;
			options.EnvironmentName = hostEnvironment.EnvironmentName;
		});

		return services;
	}

	public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
	{
		services.AddExceptionHandler<GlobalExceptionHandler>();

		// Route controller Problem/ValidationProblem results through the same enrichment below.
		services.Replace(ServiceDescriptor.Singleton<ProblemDetailsFactory, CustomProblemDetailsFactory>());

		services.AddProblemDetails(static options =>
			options.CustomizeProblemDetails = static context =>
			{
				context.ProblemDetails.Instance = context.HttpContext.Request.GetEncodedPathAndQuery();
				context.ProblemDetails.Extensions.TryAdd("method", context.HttpContext.Request.Method);

				context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
				if (context.Exception?.Message is not null)
				{
					context.ProblemDetails.Title = context.Exception?.Message;
				}

				var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;

				if (activity is not null)
				{
					context.ProblemDetails.Extensions.TryAdd("traceId", activity.TraceId.ToString());
				}
			});

		return services;
	}

	public static IServiceCollection AddVersioning(this IServiceCollection services)
	{
		services
			.AddEndpointsApiExplorer()
			.AddApiVersioning(static options =>
			{
				options.DefaultApiVersion = ApiVersion.Default;
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ReportApiVersions = true;
			})
			.AddApiExplorer(static options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			});

		return services;
	}
}
