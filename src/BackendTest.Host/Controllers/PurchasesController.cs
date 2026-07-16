using System.Net.Mime;
using System.Runtime.CompilerServices;
using Asp.Versioning;
using BackendTest.Application.Interfaces;
using BackendTest.Application.Models;
using BackendTest.Host.Extensions;
using BackendTest.Host.Mappers;
using BackendTest.Host.Requests;
using BackendTest.Host.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BackendTest.Host.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/purchases")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
public sealed class PurchasesController(
	IValidator<PurchaseAddRequest> addRequestValidator,
	IValidator<PurchaseRequest> requestValidator,
	IEntityService<PurchaseApplicationModel> entityService) : ControllerBase
{
	[HttpGet("getAll")]
	public async IAsyncEnumerable<PurchaseResponse> GetAll([EnumeratorCancellation] CancellationToken token)
	{
		await foreach (var purchase in entityService.GetAll(token).WithCancellation(token))
		{
			if (purchase?.Map() is { } response)
			{
				yield return response;
			}
		}
	}

	[HttpGet("get/{id:long}")]
	public async ValueTask<IActionResult> GetById(long id, CancellationToken token)
	{
		var purchase = await entityService.Get(id, token);

		if (purchase is null)
		{
			return Problem("Purchase not found", statusCode: StatusCodes.Status404NotFound);
		}

		return Ok(purchase);
	}

	/// <summary>
	/// Generates a CSV report of a purchase, including a list of purchased items, their prices, the total expenditure, and customer information.
	/// </summary>
	/// <param name="id">The id of the Purchase order</param>
	/// <param name="token">Cancellation token</param>
	[HttpGet("get/{id:long}/report")]
	public async Task<ActionResult<byte[]>> GetPurchaseReportById(long id, CancellationToken token)
	{
		var purchase = await entityService.Get(id, token);

		if (purchase is null)
		{
			return Problem("Purchase not found", statusCode: StatusCodes.Status404NotFound);
		}

		var report = purchase.ToCsvReport();

		return File(report, MediaTypeNames.Text.Csv, $"purchase-{id}-report.csv");
	}

	[HttpPost("add")]
	public async Task<IActionResult> Add(PurchaseAddRequest request, CancellationToken token)
	{
		var validation = await addRequestValidator.ValidateAsync(request, token);

		if (!validation.IsValid)
		{
			return this.ToValidationProblem(validation);
		}

		var purchase = request.Map()!;

		var result = await entityService.Insert(purchase, token);

		if (!result.HasValue)
		{
			return Problem("Unable to create purchase", statusCode: StatusCodes.Status400BadRequest);
		}

		var response = new { id = result.Value, version = RouteData.Values["version"] };
		return CreatedAtAction(
			nameof(GetById),
			response,
			value: response);
	}

	[HttpPost("update/{id:long}")]
	public async Task<IActionResult> Update(long id, PurchaseRequest request, CancellationToken token)
	{
		if (id != request.Id)
		{
			return Problem("Id does not match", statusCode: StatusCodes.Status400BadRequest);
		}

		var validation = await requestValidator.ValidateAsync(request, token);

		if (!validation.IsValid)
		{
			return this.ToValidationProblem(validation);
		}

		var updated = await entityService.Update(request.Map(), token);

		if (!updated)
		{
			return Problem("Unable to update purchase", statusCode: StatusCodes.Status400BadRequest);
		}

		return Ok();
	}

	[HttpDelete("delete/{id:long}")]
	[ProducesResponseType<bool>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
	public async ValueTask<ActionResult<bool>> Delete(long id)
	{
		var purchaseDeleted = await entityService.Delete(id);

		return purchaseDeleted;
	}
}
