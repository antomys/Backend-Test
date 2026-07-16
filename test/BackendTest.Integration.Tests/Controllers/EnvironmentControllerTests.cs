using System.Diagnostics;

namespace BackendTest.Integration.Tests.Controllers;

public sealed class EnvironmentControllerTests(BackendTestWebApplicationFactory factory) : IClassFixture<BackendTestWebApplicationFactory>
{
	private readonly HttpClient _client = factory.CreateClient();

	[Fact]
	public async Task GetIsProduction_ReturnsExpectedValue()
	{
		var expected = !Debugger.IsAttached || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") is "Production";

		var response = await _client.GetAsync("/environment/isproduction");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var value = await response.Content.ReadFromJsonAsync<bool>();
		value.Should().Be(expected);
	}

	[Fact]
	public async Task GetApiVersion_ReturnsOkWithExpectedPrefix()
	{
		var response = await _client.GetAsync("/environment/apiversion");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var value = await response.Content.ReadAsStringAsync();
		value.Should().StartWith("Api Version is");
	}

	[Fact]
	public async Task GetUiVersion_ReturnsOkWithExpectedPrefix()
	{
		var response = await _client.GetAsync("/environment/uiversion");

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var value = await response.Content.ReadAsStringAsync();
		value.Should().StartWith("UI Version is");
	}
}
