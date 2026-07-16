using BackendTest.Application;
using BackendTest.Host.Extensions;
using BackendTest.Host.Options;
using BackendTest.Infrastructure;
using FluentValidation;

namespace BackendTest.Host;

public sealed partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddLogging(builder.Environment);

        builder.Services.Configure<PersonValidationOptions>(builder.Configuration.GetRequiredSection(PersonValidationOptions.SectionName))
	        .AddOptionsWithValidateOnStart<PersonValidationOptions>()
	        .ValidateDataAnnotations()
	        .ValidateOnStart();

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services
            .AddRouting(static routeOptions => routeOptions.LowercaseUrls = true)
            .AddControllers();

        builder.Services
            .AddVersioning()
            .AddScalar()
            .AddInfrastructureLayer(builder.Configuration)
            .AddApplicationLayer()
            .AddExceptionHandling();

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        await app.Services.MigrateDatabase();

        app.UseExceptionHandler();

        app.UseCors();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseScalar();
        }

        app.UseHttpsRedirection();

        app.UseHealthChecks();

        app.MapControllers();

        await app.RunAsync();
    }
}
