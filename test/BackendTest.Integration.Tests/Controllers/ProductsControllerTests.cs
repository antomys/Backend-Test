using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Integration.Tests.Controllers;

public sealed class ProductsControllerTests(BackendTestWebApplicationFactory factory)
	: IClassFixture<BackendTestWebApplicationFactory>
{
	private const string BaseRoute = "/api/v1/products";

	private readonly HttpClient _client = factory.CreateClient();

	[Fact]
	public async Task GetAll_ReturnsSeededProducts()
	{
		var response = await _client.GetAsync($"{BaseRoute}/getall");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
		products.Should().Contain(product => product.Id == 1 && product.Name == "Pipe Wrench" && product.Type == "Plumbing");
	}

	[Fact]
	public async Task GetById_ExistingId_ReturnsProduct()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/1");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
		product.Should().NotBeNull();
		product!.Name.Should().Be("Pipe Wrench");
		product.Type.Should().Be("Plumbing");
	}

	[Fact]
	public async Task GetById_NonExistingId_ReturnsNotFound()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/999999");

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Product not found");
	}

	[Fact]
	public async Task Add_ValidProduct_ReturnsCreatedAndPersists()
	{
		var request = new ProductAddRequest { Name = "Test Hammer", Type = "Tools", Price = 9.99 };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/add", request);

		response.StatusCode.Should().Be(HttpStatusCode.Created);
		var created = await response.Content.ReadFromJsonAsync<CreatedEntity>();
		created!.Id.Should().BeGreaterThan(0);

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{created.Id}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
		product!.Name.Should().Be("Test Hammer");
		product.Price.Should().Be(9.99);
	}

	[Fact]
	public async Task Add_InvalidProduct_ReturnsValidationProblem()
	{
		var request = new ProductAddRequest { Name = string.Empty, Type = string.Empty, Price = -1 };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/add", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problem!.Errors.Should().ContainKeys("Name", "Type", "Price");
	}

	[Fact]
	public async Task Update_ValidProduct_ReturnsOkAndPersists()
	{
		var added = await _client.PostAsJsonAsync($"{BaseRoute}/add", new ProductAddRequest { Name = "Old Name", Type = "Tools", Price = 1 });
		var createdId = (await added.Content.ReadFromJsonAsync<CreatedEntity>())!.Id;

		var updateRequest = new ProductRequest { Id = createdId, Name = "New Name", Type = "Tools", Price = 2 };
		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/{createdId}", updateRequest);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{createdId}");
		var product = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
		product!.Name.Should().Be("New Name");
		product.Price.Should().Be(2);
	}

	[Fact]
	public async Task Update_MismatchedId_ReturnsBadRequest()
	{
		var request = new ProductRequest { Id = 2, Name = "X", Type = "Y", Price = 1 };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/1", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Id does not match");
	}

	[Fact]
	public async Task Update_NonExistingId_ReturnsBadRequest()
	{
		var request = new ProductRequest { Id = 999999, Name = "X", Type = "Y", Price = 1 };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/999999", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Unable to update product");
	}

	[Fact]
	public async Task Delete_ExistingProduct_ReturnsTrueAndRemovesIt()
	{
		var added = await _client.PostAsJsonAsync($"{BaseRoute}/add", new ProductAddRequest { Name = "Disposable", Type = "Tools", Price = 1 });
		var createdId = (await added.Content.ReadFromJsonAsync<CreatedEntity>())!.Id;

		var response = await _client.DeleteAsync($"{BaseRoute}/delete/{createdId}");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var deleted = await response.Content.ReadFromJsonAsync<bool>();
		deleted.Should().BeTrue();

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{createdId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Delete_NonExistingProduct_ReturnsFalse()
	{
		var response = await _client.DeleteAsync($"{BaseRoute}/delete/999999");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var deleted = await response.Content.ReadFromJsonAsync<bool>();
		deleted.Should().BeFalse();
	}

	private sealed record CreatedEntity(long Id, string? Version);
}
