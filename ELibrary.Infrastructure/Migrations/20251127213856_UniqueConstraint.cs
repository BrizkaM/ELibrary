using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELibrary.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Book_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Book_ISBN",
                table: "Books");
        }
    }
}
