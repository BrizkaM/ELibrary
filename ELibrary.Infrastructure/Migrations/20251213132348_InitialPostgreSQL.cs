using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ELibrary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Author = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Year = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ISBN = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ActualQuantity = table.Column<int>(type: "integer", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BorrowBookRecords",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    BookID = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowBookRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BorrowBookRecords_Books_BookID",
                        column: x => x.BookID,
                        principalTable: "Books",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "ID", "ActualQuantity", "Author", "ISBN", "Name", "Year" },
                values: new object[,]
                {
                    { new Guid("c3984d72-57a4-432d-88b1-38290f93450a"), 3, "Christopher Ruocchio", "9780756419271", "Howling Dark", new DateTime(2019, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c3984d72-57a4-432d-88b1-38290f93450b"), 3, "Christopher Ruocchio", "9780756419288", "Demon in white", new DateTime(2020, 11, 24, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c3984d72-57a4-432d-88b1-38290f93450e"), 3, "Christopher Ruocchio", "9780756419264", "Empire of Silence", new DateTime(2018, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Book_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BorrowBookRecords_BookID",
                table: "BorrowBookRecords",
                column: "BookID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BorrowBookRecords");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
