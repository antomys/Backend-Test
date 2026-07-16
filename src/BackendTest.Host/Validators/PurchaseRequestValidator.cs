using BackendTest.Host.Requests;
using FluentValidation;

namespace BackendTest.Host.Validators;

public sealed class PurchaseRequestValidator : AbstractValidator<PurchaseRequest>
{
	public PurchaseRequestValidator()
	{
		RuleFor(static request => request.CustomerId).GreaterThan(0);

		RuleFor(static request => request.ProductIds).NotEmpty();
	}
}
