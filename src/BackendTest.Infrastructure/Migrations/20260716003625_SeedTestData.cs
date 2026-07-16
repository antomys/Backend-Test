using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendTest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var seededAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

            migrationBuilder.InsertData(
                table: "persons",
                columns: new[] { "Id", "CreatedAt", "UpdatedAt", "FirstName", "LastName", "YearOfBirth" },
                values: new object[,]
                {
                    { 1L, seededAt, seededAt, "John", "Doe", new DateOnly(1980, 1, 1) },
                    { 2L, seededAt, seededAt, "Jane", "Doe", new DateOnly(1985, 1, 1) },
                    { 3L, seededAt, seededAt, "Bob", "Smith", new DateOnly(1990, 1, 1) },
                    { 4L, seededAt, seededAt, "Alice", "Johnson", new DateOnly(1995, 1, 1) },
                    { 5L, seededAt, seededAt, "Mike", "Brown", new DateOnly(1982, 1, 1) },
                    { 6L, seededAt, seededAt, "Samantha", "Davis", new DateOnly(1987, 1, 1) },
                    { 7L, seededAt, seededAt, "David", "Wilson", new DateOnly(1992, 1, 1) },
                    { 8L, seededAt, seededAt, "Emily", "Taylor", new DateOnly(1997, 1, 1) },
                    { 9L, seededAt, seededAt, "Chris", "Anderson", new DateOnly(1984, 1, 1) },
                    { 10L, seededAt, seededAt, "Jessica", "Thomas", new DateOnly(1989, 1, 1) },
                });

            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "Id", "CreatedAt", "UpdatedAt", "Name", "Type", "Price" },
                values: new object[,]
                {
                    { 1L, seededAt, seededAt, "Pipe Wrench", "Plumbing", 19.99d },
                    { 2L, seededAt, seededAt, "Electric Drill", "Electric", 0d },
                    { 3L, seededAt, seededAt, "Garden Hose", "Gardening", 4.99d },
                    { 4L, seededAt, seededAt, "Toilet Plunger", "Plumbing", 1.49d },
                    { 5L, seededAt, seededAt, "Electric Screwdriver", "Electric", 0d },
                    { 6L, seededAt, seededAt, "Garden Shovel", "Gardening", 0d },
                    { 7L, seededAt, seededAt, "Faucet", "Plumbing", 0d },
                    { 8L, seededAt, seededAt, "Electric Saw", "Electric", 0d },
                    { 9L, seededAt, seededAt, "Garden Gloves", "Gardening", 0d },
                    { 10L, seededAt, seededAt, "Pipe Cutter", "Plumbing", 0d },
                });

            // Purchase 1 folds together what would otherwise be three single-product
            // purchases for customer 1 (products 1, 2, 3), so it seeds a multi-product
            // purchase; the remaining purchases are numbered sequentially after it.
            migrationBuilder.InsertData(
                table: "purchases",
                columns: new[] { "Id", "CreatedAt", "UpdatedAt", "CustomerId" },
                values: new object[,]
                {
                    { 1L, seededAt, seededAt, 1L },
                    { 2L, seededAt, seededAt, 2L },
                    { 3L, seededAt, seededAt, 2L },
                    { 4L, seededAt, seededAt, 3L },
                    { 5L, seededAt, seededAt, 7L },
                    { 6L, seededAt, seededAt, 7L },
                    { 7L, seededAt, seededAt, 4L },
                    { 8L, seededAt, seededAt, 4L },
                    { 9L, seededAt, seededAt, 4L },
                    { 10L, seededAt, seededAt, 4L },
                    { 11L, seededAt, seededAt, 8L },
                    { 12L, seededAt, seededAt, 8L },
                    { 13L, seededAt, seededAt, 5L },
                    { 14L, seededAt, seededAt, 5L },
                    { 15L, seededAt, seededAt, 8L },
                    { 16L, seededAt, seededAt, 1L },
                    { 17L, seededAt, seededAt, 2L },
                    { 18L, seededAt, seededAt, 3L },
                    { 19L, seededAt, seededAt, 4L },
                    { 20L, seededAt, seededAt, 5L },
                    { 21L, seededAt, seededAt, 1L },
                    { 22L, seededAt, seededAt, 2L },
                    { 23L, seededAt, seededAt, 3L },
                    { 24L, seededAt, seededAt, 4L },
                    { 25L, seededAt, seededAt, 5L },
                    { 26L, seededAt, seededAt, 1L },
                    { 27L, seededAt, seededAt, 2L },
                    { 28L, seededAt, seededAt, 3L },
                    { 29L, seededAt, seededAt, 4L },
                    { 30L, seededAt, seededAt, 5L },
                    { 31L, seededAt, seededAt, 1L },
                    { 32L, seededAt, seededAt, 2L },
                    { 33L, seededAt, seededAt, 3L },
                    { 34L, seededAt, seededAt, 4L },
                    { 35L, seededAt, seededAt, 6L },
                    { 36L, seededAt, seededAt, 6L },
                    { 37L, seededAt, seededAt, 6L },
                });

            migrationBuilder.InsertData(
                table: "purchase_products",
                columns: new[] { "ProductsId", "PurchasePersistenceModelId" },
                values: new object[,]
                {
                    { 1L, 1L },
                    { 2L, 1L },
                    { 3L, 1L },
                    { 4L, 2L },
                    { 5L, 3L },
                    { 6L, 4L },
                    { 7L, 5L },
                    { 8L, 6L },
                    { 9L, 7L },
                    { 10L, 8L },
                    { 4L, 9L },
                    { 8L, 10L },
                    { 8L, 11L },
                    { 2L, 12L },
                    { 1L, 13L },
                    { 6L, 14L },
                    { 5L, 15L },
                    { 4L, 16L },
                    { 6L, 17L },
                    { 10L, 18L },
                    { 3L, 19L },
                    { 1L, 20L },
                    { 6L, 21L },
                    { 10L, 22L },
                    { 7L, 23L },
                    { 1L, 24L },
                    { 6L, 25L },
                    { 10L, 26L },
                    { 7L, 27L },
                    { 1L, 28L },
                    { 6L, 29L },
                    { 10L, 30L },
                    { 7L, 31L },
                    { 1L, 32L },
                    { 6L, 33L },
                    { 10L, 34L },
                    { 1L, 35L },
                    { 4L, 36L },
                    { 7L, 37L },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "purchase_products",
                keyColumns: new[] { "ProductsId", "PurchasePersistenceModelId" },
                keyValues: new object[,]
                {
                    { 1L, 1L },
                    { 2L, 1L },
                    { 3L, 1L },
                    { 4L, 2L },
                    { 5L, 3L },
                    { 6L, 4L },
                    { 7L, 5L },
                    { 8L, 6L },
                    { 9L, 7L },
                    { 10L, 8L },
                    { 4L, 9L },
                    { 8L, 10L },
                    { 8L, 11L },
                    { 2L, 12L },
                    { 1L, 13L },
                    { 6L, 14L },
                    { 5L, 15L },
                    { 4L, 16L },
                    { 6L, 17L },
                    { 10L, 18L },
                    { 3L, 19L },
                    { 1L, 20L },
                    { 6L, 21L },
                    { 10L, 22L },
                    { 7L, 23L },
                    { 1L, 24L },
                    { 6L, 25L },
                    { 10L, 26L },
                    { 7L, 27L },
                    { 1L, 28L },
                    { 6L, 29L },
                    { 10L, 30L },
                    { 7L, 31L },
                    { 1L, 32L },
                    { 6L, 33L },
                    { 10L, 34L },
                    { 1L, 35L },
                    { 4L, 36L },
                    { 7L, 37L },
                });

            migrationBuilder.DeleteData(
                table: "purchases",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 10L,
                    11L, 12L, 13L, 14L, 15L, 16L, 17L, 18L, 19L, 20L,
                    21L, 22L, 23L, 24L, 25L, 26L, 27L, 28L, 29L, 30L,
                    31L, 32L, 33L, 34L, 35L, 36L, 37L,
                });

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 10L });

            migrationBuilder.DeleteData(
                table: "persons",
                keyColumn: "Id",
                keyValues: new object[] { 1L, 2L, 3L, 4L, 5L, 6L, 7L, 8L, 9L, 10L });
        }
    }
}
