using System.Reflection;
using BackendTest.Host.Transformers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace BackendTest.Host.Extensions;

public static class ScalarExtensions
{
	private static readonly string _version = Assembly
		.GetEntryAssembly()
		?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
		?.InformationalVersion ?? "v1";

	public static IServiceCollection AddScalar(
		this IServiceCollection services,
		Action<OpenApiOptions>? configureOptions = null)
	{
		Action<OpenApiOptions> basicConfiguration = static options =>
		{
			options.AddScalarTransformers();
			options.AddSchemaTransformer<AutoFixtureOpenApiSchemaTransformer>();
			options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
			options.ShouldInclude = static _ => true;
		};

		if (configureOptions is not null)
		{
			basicConfiguration += configureOptions;
		}

		return services.AddOpenApi(_version, basicConfiguration);
	}

	public static IEndpointRouteBuilder UseScalar(
		this IEndpointRouteBuilder routeBuilder,
		Action<ScalarOptions, HttpContext>? configureOptions = null)
	{
		var configuration = ConfigureBasicScalar;

		if (configureOptions is not null)
		{
			configuration += configureOptions;
		}

		routeBuilder
			.MapScalarApiReference(configuration);

		return routeBuilder;
	}

	private static void ConfigureBasicScalar(ScalarOptions options, HttpContext context)
	{
		options
			.WithTitle($"{AppDomain.CurrentDomain.FriendlyName} | {_version}")
			.EnableDarkMode()
			.HideDarkModeToggle()
			.AddDocument(_version);

		options.Servers = [];
		options.DynamicBaseServerUrl = true;
	}
}
