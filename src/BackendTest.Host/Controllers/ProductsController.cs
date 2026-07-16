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
[Route("api/v{version:apiVersion}/products")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.Json)]
public sealed class ProductController(
	IValidator<ProductAddRequest> addRequestValidator,
	IValidator<ProductRequest> requestValidator,
	IEntityService<ProductApplicationModel> entityService) : ControllerBase
{
	[HttpGet("getAll")]
	public async IAsyncEnumerable<ProductResponse> GetAll([EnumeratorCancellation] CancellationToken token)
	{
		await foreach (var person in entityService.GetAll(token).WithCancellation(token))
		{
			if (person?.Map() is { } response)
			{
				yield return response;
			}
		}
	}

	[HttpGet("get/{id:long}")]
	public async ValueTask<IActionResult> GetById(long id, CancellationToken token)
	{
		var person = await entityService.Get(id, token);

		if (person is null)
		{
			return Problem("Product not found", statusCode: StatusCodes.Status404NotFound);
		}

		return Ok(person);
	}

	[HttpPost("add")]
	public async Task<IActionResult> Add(ProductAddRequest request, CancellationToken token)
	{
		var validation = await addRequestValidator.ValidateAsync(request, token);

		if (!validation.IsValid)
		{
			return this.ToValidationProblem(validation);
		}

		var bookingRequest = request.Map()!;

		var result = await entityService.Insert(bookingRequest, token);

		if (!result.HasValue)
		{
			return Problem("Unable to create product", statusCode: StatusCodes.Status400BadRequest);
		}

		var response = new { id = result.Value, version = RouteData.Values["version"] };
		return CreatedAtAction(
			nameof(GetById),
			response,
			value: response);
	}

	[HttpPost("update/{id:long}")]
	public async Task<IActionResult> Update(long id, ProductRequest request, CancellationToken token)
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
			return Problem("Unable to update product", statusCode: StatusCodes.Status400BadRequest);
		}

		return Ok();
	}

	[HttpDelete("delete/{id:long}")]
	[ProducesResponseType<bool>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
	public async ValueTask<ActionResult<bool>> Delete(long id)
	{
		var bookingDeleted = await entityService.Delete(id);

		return bookingDeleted;
	}
}
