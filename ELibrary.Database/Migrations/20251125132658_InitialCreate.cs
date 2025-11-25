using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ELibrary.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Year = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ISBN = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ActualQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Idate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Udate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "ID", "ActualQuantity", "Author", "ISBN", "Idate", "Name", "Udate", "Year" },
                values: new object[,]
                {
                    { new Guid("c3984d72-57a4-432d-88b1-38290f93450a"), 3, "Christopher Ruocchio", "9780756419271", new DateTime(2025, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc), "Howling Dark", new DateTime(2025, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c3984d72-57a4-432d-88b1-38290f93450b"), 3, "Christopher Ruocchio", "9780756419288", new DateTime(2025, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc), "Demon in white", new DateTime(2025, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2020, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c3984d72-57a4-432d-88b1-38290f93450e"), 3, "Christopher Ruocchio", "9780756419264", new DateTime(2025, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc), "Empire of Silence", new DateTime(2025, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2018, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
