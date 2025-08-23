using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PdfExtractor.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProcessedPropertie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_processed",
                table: "documents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_processed",
                table: "documents");
        }
    }
}
