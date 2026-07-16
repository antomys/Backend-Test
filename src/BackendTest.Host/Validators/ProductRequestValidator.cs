using BackendTest.Host.Requests;
using FluentValidation;

namespace BackendTest.Host.Validators;

public sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
	public ProductRequestValidator()
	{
		RuleFor(static request => request.Name)
			.NotEmpty();

		RuleFor(static request => request.Type)
			.NotEmpty();

		RuleFor(static request => request.Price)
			.GreaterThanOrEqualTo(0);
	}
}
