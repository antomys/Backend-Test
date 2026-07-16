using System.Runtime.InteropServices;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Host.Extensions;

internal static class ValidationResultExtensions
{
	/// <summary>
	/// Converts a failed FluentValidation result into a problem response that flows through the
	/// configured <see cref="Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory"/>,
	/// so the payload matches the format produced by the global exception handler.
	/// </summary>
	public static IActionResult ToValidationProblem(this ControllerBase controller, ValidationResult validationResult)
	{
		foreach (ref var error in CollectionsMarshal.AsSpan(validationResult.Errors))
		{
			controller.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
		}

		return controller.ValidationProblem();
	}
}
