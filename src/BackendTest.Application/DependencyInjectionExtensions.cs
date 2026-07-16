using BackendTest.Application.Interfaces;
using BackendTest.Application.Models;
using BackendTest.Application.Services;
using BackendTest.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BackendTest.Application;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
	{
		services.TryAddScoped<IEntityService<PersonApplicationModel>, PersonService>();
		services.TryAddScoped<IEntityService<ProductApplicationModel>, ProductService>();
		services.TryAddScoped<IEntityService<PurchaseApplicationModel>, PurchaseService>();

		return services;
	}
}
