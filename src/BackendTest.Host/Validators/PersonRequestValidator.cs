using BackendTest.Host.Options;
using BackendTest.Host.Requests;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace BackendTest.Host.Validators;

public sealed class PersonRequestValidator : AbstractValidator<PersonRequest>
{
	public PersonRequestValidator(IOptionsMonitor<PersonValidationOptions> personValidationOptions)
	{
		RuleFor(static request => request.FirstName).NotEmpty();
		RuleFor(static request => request.LastName).NotEmpty();
		RuleFor(static request => request.Id).NotNull();

		RuleFor(static request => request.YearOfBirth)
			.Must(yearOfBirth => CalculateAge(yearOfBirth) >= personValidationOptions.CurrentValue.MinimumAge)
			.WithMessage(_ => $"Person must be at least {personValidationOptions.CurrentValue.MinimumAge} years old.");
	}

	private static int CalculateAge(DateOnly yearOfBirth)
	{
		var today = DateOnly.FromDateTime(DateTime.UtcNow);
		var age = today.Year - yearOfBirth.Year;

		if (yearOfBirth > today.AddYears(-age))
		{
			age--;
		}

		return age;
	}
}
