using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Integration.Tests.Controllers;

public sealed class PersonControllerTests(BackendTestWebApplicationFactory factory) : IClassFixture<BackendTestWebApplicationFactory>
{
	private const string BaseRoute = "/api/v1/persons";

	private readonly HttpClient _client = factory.CreateClient();

	[Fact]
	public async Task GetAll_ReturnsSeededPersons()
	{
		var response = await _client.GetAsync($"{BaseRoute}/getall");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var persons = await response.Content.ReadFromJsonAsync<List<PersonResponse>>();
		persons.Should().Contain(person => person.Id == 1 && person.FirstName == "John" && person.LastName == "Doe");
	}

	[Fact]
	public async Task GetById_ExistingId_ReturnsPerson()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/1");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var person = await response.Content.ReadFromJsonAsync<PersonResponse>();
		person!.FirstName.Should().Be("John");
		person.LastName.Should().Be("Doe");
	}

	[Fact]
	public async Task GetById_NonExistingId_ReturnsNotFound()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/999999");

		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Person not found");
	}

	[Fact]
	public async Task GetById_ZeroId_ReturnsBadRequest()
	{
		var response = await _client.GetAsync($"{BaseRoute}/get/0");

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Add_ValidAdult_ReturnsCreatedAndPersists()
	{
		var request = new PersonAddRequest { FirstName = "Test", LastName = "User", YearOfBirth = new DateOnly(2000, 1, 1) };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/add", request);

		response.StatusCode.Should().Be(HttpStatusCode.Created);
		var created = await response.Content.ReadFromJsonAsync<CreatedEntity>();
		created!.Id.Should().BeGreaterThan(0);

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{created.Id}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var person = await getResponse.Content.ReadFromJsonAsync<PersonResponse>();
		person!.FirstName.Should().Be("Test");
	}

	[Fact]
	public async Task Add_Underage_ReturnsValidationProblem()
	{
		var request = new PersonAddRequest
		{
			FirstName = "Kid",
			LastName = "User",
			YearOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)),
		};

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/add", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problem!.Errors.Should().ContainKey("YearOfBirth");
	}

	[Fact]
	public async Task Add_MissingNames_ReturnsValidationProblem()
	{
		var request = new PersonAddRequest { FirstName = string.Empty, LastName = string.Empty, YearOfBirth = new DateOnly(2000, 1, 1) };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/add", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
		problem!.Errors.Should().ContainKeys("FirstName", "LastName");
	}

	[Fact]
	public async Task Update_ValidPerson_ReturnsOkAndPersists()
	{
		var added = await _client.PostAsJsonAsync(
			$"{BaseRoute}/add",
			new PersonAddRequest { FirstName = "Old", LastName = "Name", YearOfBirth = new DateOnly(2000, 1, 1) });
		var createdId = (await added.Content.ReadFromJsonAsync<CreatedEntity>())!.Id;

		var updateRequest = new PersonRequest { Id = createdId, FirstName = "New", LastName = "Name", YearOfBirth = new DateOnly(2000, 1, 1) };
		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/{createdId}", updateRequest);

		response.StatusCode.Should().Be(HttpStatusCode.OK);

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{createdId}");
		var person = await getResponse.Content.ReadFromJsonAsync<PersonResponse>();
		person!.FirstName.Should().Be("New");
	}

	[Fact]
	public async Task Update_MismatchedId_ReturnsBadRequest()
	{
		var request = new PersonRequest { Id = 2, FirstName = "X", LastName = "Y", YearOfBirth = new DateOnly(2000, 1, 1) };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/1", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Id does not match");
	}

	[Fact]
	public async Task Update_NonExistingId_ReturnsBadRequest()
	{
		var request = new PersonRequest { Id = 999999, FirstName = "X", LastName = "Y", YearOfBirth = new DateOnly(2000, 1, 1) };

		var response = await _client.PostAsJsonAsync($"{BaseRoute}/update/999999", request);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem!.Detail.Should().Be("Unable to update person");
	}

	[Fact]
	public async Task Delete_ExistingPerson_ReturnsTrueAndRemovesIt()
	{
		var added = await _client.PostAsJsonAsync(
			$"{BaseRoute}/add",
			new PersonAddRequest { FirstName = "Disposable", LastName = "Person", YearOfBirth = new DateOnly(2000, 1, 1) });
		var createdId = (await added.Content.ReadFromJsonAsync<CreatedEntity>())!.Id;

		var response = await _client.DeleteAsync($"{BaseRoute}/delete/{createdId}");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var deleted = await response.Content.ReadFromJsonAsync<bool>();
		deleted.Should().BeTrue();

		var getResponse = await _client.GetAsync($"{BaseRoute}/get/{createdId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task Delete_NonExistingPerson_ReturnsFalse()
	{
		var response = await _client.DeleteAsync($"{BaseRoute}/delete/999999");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var deleted = await response.Content.ReadFromJsonAsync<bool>();
		deleted.Should().BeFalse();
	}

	private sealed record CreatedEntity(long Id, string? Version);
}
