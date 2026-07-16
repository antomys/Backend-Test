using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Integration.Tests.Controllers;

public sealed class PurchasesControllerTests(BackendTestWebApplicationFactory factory) : IClassFixture<BackendTestWebApplicationFactory>
{
	private const string BaseRoute = "/api/v1/purchases";

	private readonly HttpClient _client = factory.CreateClient();

	[Fact]
	public async Task GetAll_ReturnsSeededPurchases()
	{
		var response = await _client.GetAsync($"{BaseRoute}/getall");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var purchases = await response.Content.ReadFromJsonAsync<List<PurchaseResponse>>();
		purchases.Should().Contain(purchase =>
			purchase.Id == 1 && purchase.Customer.Id == 1 && purchase.Products.Any(product => product.Id == 1));
	}

	[Fact]
	public async Task GetById_ExistingId_ReturnsPurchase()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/1");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var purchase = await response.Content.ReadFromJsonAsync<PurchaseResponse>();
		purchase!.Customer.FirstName.Should().Be("John");
		purchase.Products.Should().Contain(product => product.Id == 1 && product.Name == "Pipe Wrench");
	}

	[Fact]
	public async Task GetById_NonExistingId_ReturnsNotFound()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/999999");

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Purchase not found");
	}

	[Fact]
	public async Task GetPurchaseReportById_ExistingId_ReturnsCsvFile()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/1/report");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
		response.Content.Headers.ContentDisposition!.FileName.Should().Be("purchase-1-report.csv");

		var csv = await response.Content.ReadAsStringAsync();
		csv.Should().Contain("CustomerName:;John Doe");
		csv.Should().Contain("Pipe Wrench");
	}

	[Fact]
	public async Task GetPurchaseReportById_NonExistingId_ReturnsNotFound()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/999999/report");

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Purchase not found");
	}

	[Fact]
	public async Task Add_ValidPurchase_ReturnsCreatedAndPersists()
	{
		var request = new PurchaseAddRequest { CustomerId = 1, ProductIds = [1, 2] };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/add", request);

		response.StatusCode.Should().Be(HttpStatusCode.Created);
		var created = await response.Content.ReadFromJsonAsync<CreatedEntity>();
		created!.Id.Should().BeGreaterThan(0);

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{created.Id}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var purchase = await getResponse.Content.ReadFromJsonAsync<PurchaseResponse>();
		purchase!.Customer.Id.Should().Be(1);
		purchase.Products.Select(static product => product.Id).Should().BeEquivalentTo([1, 2]);
	}

	[Fact]
	public async Task Add_InvalidPurchase_ReturnsValidationProblem()
	{
		var request = new PurchaseAddRequest { CustomerId = 0, ProductIds = [] };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/add", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problem!.Errors.Should().ContainKeys("CustomerId", "ProductIds");
	}

	[Fact]
	public async Task Update_ValidPurchase_ReturnsOkAndPersists()
	{
		var added = await _client.PostAsJsonAsync($"{BaseRoute}/add", new PurchaseAddRequest { CustomerId = 1, ProductIds = [1] });
		var createdId = (await added.Content.ReadFromJsonAsync<CreatedEntity>())!.Id;

		var updateRequest = new PurchaseRequest { Id = createdId, CustomerId = 2, ProductIds = [3] };
		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/{createdId}", updateRequest);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{createdId}");
		var purchase = await getResponse.Content.ReadFromJsonAsync<PurchaseResponse>();
		purchase!.Customer.Id.Should().Be(2);
		purchase.Products.Select(static product => product.Id).Should().BeEquivalentTo([3]);
	}

	[Fact]
	public async Task Update_MismatchedId_ReturnsBadRequest()
	{
		var request = new PurchaseRequest { Id = 2, CustomerId = 1, ProductIds = [1] };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/1", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Id does not match");
	}

	[Fact]
	public async Task Update_NonExistingId_ReturnsBadRequest()
	{
		var request = new PurchaseRequest { Id = 999999, CustomerId = 1, ProductIds = [1] };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/999999", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Unable to update purchase");
	}

	[Fact]
	public async Task Delete_ExistingPurchase_ReturnsTrueAndRemovesIt()
	{
		var added = await _client.PostAsJsonAsync($"{BaseRoute}/add", new PurchaseAddRequest { CustomerId = 1, ProductIds = [1] });
		var createdId = (await added.Content.ReadFromJsonAsync<CreatedEntity>())!.Id;

		var response = await _client.DeleteAsync($"{BaseRoute}/delete/{createdId}");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var deleted = await response.Content.ReadFromJsonAsync<bool>();
		deleted.Should().BeTrue();

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{createdId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Delete_NonExistingPurchase_ReturnsFalse()
	{
		var response = await _client.DeleteAsync($"{BaseRoute}/delete/999999");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var deleted = await response.Content.ReadFromJsonAsync<bool>();
		deleted.Should().BeFalse();
	}

	private sealed record CreatedEntity(long Id, string? Version);
}
