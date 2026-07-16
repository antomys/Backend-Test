using System.Text.Json;
using System.Text.Json.Nodes;
using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace BackendTest.Host.Transformers;

public sealed class AutoFixtureOpenApiSchemaTransformer : IOpenApiSchemaTransformer
{
	private static readonly Fixture _fixture = CreateFixture();

	public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
	{
		if (schema.Example != null
		    || context.JsonTypeInfo.Type is not { IsClass: true, IsAbstract: false } type
		    || type == typeof(string))
		{
			return Task.CompletedTask;
		}

		try
		{
			var contextInstance = new SpecimenContext(_fixture);
			var instance = contextInstance.Resolve(type);

			if (instance is not null and not OmitSpecimen)
			{
				var json = JsonSerializer.Serialize(instance, context.JsonTypeInfo.Options);
				schema.Example = JsonNode.Parse(json);
			}
		}
		catch
		{
			// Ignore AutoFixture or Serialization exceptions to not crash the OpenAPI generation
		}

		return Task.CompletedTask;
	}

	private static Fixture CreateFixture()
	{
		var fixture = new Fixture();
		fixture.Behaviors.Remove(fixture.Behaviors.OfType<ThrowingRecursionBehavior>().FirstOrDefault());
		fixture.Behaviors.Add(new OmitOnRecursionBehavior());
		return fixture;
	}
}
