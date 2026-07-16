using System.Globalization;
using BackendTest.Application.Models;
using BackendTest.Host.Requests;
using BackendTest.Host.Responses;
using Sylvan.Data;
using Sylvan.Data.Csv;

namespace BackendTest.Host.Mappers;

internal static class PurchaseMapper
{
	private static readonly NumberFormatInfo _priceFormat = new() { NumberDecimalSeparator = "," };

	public static PurchaseApplicationModel? Map(
		this PurchaseAddRequest? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PurchaseApplicationModel
		{
			Customer = new PersonApplicationModel { Id = model.CustomerId },
			Products = model.ProductIds.Select(static product => new ProductApplicationModel { Id = product }).ToArray(),
		};
	}

	public static PurchaseApplicationModel? Map(
		this PurchaseRequest? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PurchaseApplicationModel
		{
			Id = model.Id,
			Customer = new PersonApplicationModel { Id = model.CustomerId },
			Products = model.ProductIds.Select(static product => new ProductApplicationModel { Id = product }).ToArray(),
		};
	}

	public static PurchaseResponse? Map(
		this PurchaseApplicationModel? model)
	{
		if (model is null)
		{
			return null;
		}

		return new PurchaseResponse
		{
			Id = model.Id,
			Customer = model.Customer.Map(),
			Products = model.Products.Select(static product => product.Map()),
		};
	}

	public static byte[] ToCsvReport(this PurchaseApplicationModel purchase)
	{
		var lines = purchase.Products
			.GroupBy(static product => product.Id)
			.Select(static group => new
			{
				ProductId = group.Key,
				Count = group.Count(),
				ProductName = group.First().Name,
				Price = group.First().Price.ToString("0.00", _priceFormat),
			});

		using var stream = new MemoryStream();
		using (var writer = new StreamWriter(stream, leaveOpen: true))
		{
			writer.Write("CustomerName:;");
			writer.Write(purchase.Customer.FirstName);
			writer.Write(' ');
			writer.Write(purchase.Customer.LastName);
			writer.Write("\r\n");

			using var reader = lines.AsDataReader();
			using var csvWriter = CsvDataWriter.Create(writer, new CsvDataWriterOptions { Delimiter = ';' });
			csvWriter.Write(reader);
		}

		return stream.ToArray();
	}
}
