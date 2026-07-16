using BackendTest.Host.Requests;
using FluentValidation;

namespace BackendTest.Host.Validators;

public sealed class ProductAddRequestValidator : AbstractValidator<ProductAddRequest>
{
	public ProductAddRequestValidator()
	{
		RuleFor(static request => request.Name)
			.NotEmpty();

		RuleFor(static request => request.Type)
			.NotEmpty();

		RuleFor(static request => request.Price)
			.GreaterThanOrEqualTo(0);
	}
}
