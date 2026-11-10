using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IRM.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVisaSymbol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VisaSymbol",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisaSymbol",
                table: "ArchivedEmployees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisaSymbol",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "VisaSymbol",
                table: "ArchivedEmployees");
        }
    }
}
