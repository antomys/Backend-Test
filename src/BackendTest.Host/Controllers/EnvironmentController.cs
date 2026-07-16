using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Host.Controllers;

[Route("environment")]
[ApiController]
public sealed class EnvironmentController : ControllerBase
{
	private const string EnvironmentKey = "ASPNETCORE_ENVIRONMENT";
	private const string ApiVersionKey = "API_VERSION";
	private const string UiVersionKey = "UI_VERSION";

	[HttpGet("isproduction")]
	public ActionResult<bool> GetIsProduction()
	{
		return !Debugger.IsAttached || Environment.GetEnvironmentVariable(EnvironmentKey) is "Production";
	}

	[HttpGet("apiversion")]
	public ActionResult<string> GetApiVersion()
	{
		return $"Api Version is {Environment.GetEnvironmentVariable(ApiVersionKey)}";
	}

	[HttpGet("uiversion")]
	public ActionResult<string> GetUiVersion()
	{
		return $"UI Version is {Environment.GetEnvironmentVariable(UiVersionKey)}";
	}
}
