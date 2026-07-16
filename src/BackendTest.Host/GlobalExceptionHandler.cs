using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Host;

public sealed class GlobalExceptionHandler(
	IProblemDetailsService problemDetailsService,
	ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
	private const string DefaultErrorMessage = "An unexpected error occurred.";

	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		var problemDetails = new ProblemDetails();

		switch (exception)
		{
			case ArgumentNullException argumentNullException:
				problemDetails.Title = argumentNullException.Message;
				problemDetails.Status = StatusCodes.Status400BadRequest;
				break;
			case NotImplementedException notImplementedException:
				problemDetails.Title = notImplementedException.Message;
				problemDetails.Status = StatusCodes.Status501NotImplemented;
				break;
			default:
				problemDetails.Title = DefaultErrorMessage;
				problemDetails.Status = StatusCodes.Status500InternalServerError;
				break;
		}

		// Server-side gets the full detail; the response stays a safe, generic title.
		logger.Log(
			problemDetails.Status >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning,
			exception,
			"Unhandled exception while handling {Method} {Path}.",
			httpContext.Request.Method,
			httpContext.Request.Path);

		httpContext.Response.StatusCode = problemDetails.Status.Value;

		// Writing through IProblemDetailsService runs the CustomizeProblemDetails
		// enrichment (requestId, traceId, method, instance) configured in Program.
		return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
		{
			HttpContext = httpContext,
			ProblemDetails = problemDetails,
			Exception = exception,
		});
	}
}
