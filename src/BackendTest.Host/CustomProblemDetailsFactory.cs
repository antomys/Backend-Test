using BackendTest.Host;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace BackendTest.Host;

/// <summary>
/// Produces problem responses for controller results (<c>Problem(...)</c> and
/// <c>ValidationProblem(...)</c>) through the same <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/>
/// enrichment used by <see cref="GlobalExceptionHandler"/>, so every error payload shares one format.
/// </summary>
internal sealed class CustomProblemDetailsFactory(
	IOptions<ApiBehaviorOptions> apiBehaviorOptions,
	IOptions<ProblemDetailsOptions> problemDetailsOptions) : ProblemDetailsFactory
{
	private readonly ApiBehaviorOptions _apiBehaviorOptions = apiBehaviorOptions.Value;
	private readonly Action<ProblemDetailsContext>? _customize = problemDetailsOptions.Value.CustomizeProblemDetails;

	public override ProblemDetails CreateProblemDetails(
		HttpContext httpContext,
		int? statusCode = null,
		string? title = null,
		string? type = null,
		string? detail = null,
		string? instance = null)
	{
		statusCode ??= StatusCodes.Status500InternalServerError;

		var problemDetails = new ProblemDetails
		{
			Status = statusCode,
			Title = title,
			Type = type,
			Detail = detail,
			Instance = instance,
		};

		ApplyDefaults(httpContext, problemDetails, statusCode.Value);

		return problemDetails;
	}

	public override ValidationProblemDetails CreateValidationProblemDetails(
		HttpContext httpContext,
		ModelStateDictionary modelStateDictionary,
		int? statusCode = null,
		string? title = null,
		string? type = null,
		string? detail = null,
		string? instance = null)
	{
		ArgumentNullException.ThrowIfNull(modelStateDictionary);

		statusCode ??= StatusCodes.Status400BadRequest;

		var problemDetails = new ValidationProblemDetails(modelStateDictionary)
		{
			Status = statusCode,
			Type = type,
			Detail = detail,
			Instance = instance,
		};

		// Preserve the framework's default validation title unless an explicit one is supplied.
		if (title is not null)
		{
			problemDetails.Title = title;
		}

		ApplyDefaults(httpContext, problemDetails, statusCode.Value);

		return problemDetails;
	}

	private void ApplyDefaults(HttpContext httpContext, ProblemDetails problemDetails, int statusCode)
	{
		problemDetails.Status ??= statusCode;

		if (_apiBehaviorOptions.ClientErrorMapping.TryGetValue(statusCode, out var clientErrorData))
		{
			problemDetails.Title ??= clientErrorData.Title;
			problemDetails.Type ??= clientErrorData.Link;
		}

		_customize?.Invoke(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			ProblemDetails = problemDetails,
		});
	}
}
