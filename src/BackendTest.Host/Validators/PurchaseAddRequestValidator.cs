using BackendTest.Host.Requests;
using FluentValidation;

namespace BackendTest.Host.Validators;

public sealed class PurchaseAddRequestValidator : AbstractValidator<PurchaseAddRequest>
{
	public PurchaseAddRequestValidator()
	{
		RuleFor(static request => request.CustomerId).GreaterThan(0);

		RuleFor(static request => request.ProductIds).NotEmpty();
	}
}
