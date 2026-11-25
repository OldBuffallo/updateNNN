using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IRM.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmployeeCompanyNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Companies_IDCompany",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IDCompany",
                table: "Employees");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "ImportHistories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IDCompany",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "IDCompany",
                table: "ArchivedEmployees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IDCompany",
                table: "Employees",
                column: "IDCompany");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Companies_IDCompany",
                table: "Employees",
                column: "IDCompany",
                principalTable: "Companies",
                principalColumn: "IDCompany",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Companies_IDCompany",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IDCompany",
                table: "Employees");

            migrationBuilder.AlterColumn<int>(
                name: "CompanyId",
                table: "ImportHistories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IDCompany",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "IDCompany",
                table: "ArchivedEmployees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IDCompany",
                table: "Employees",
                column: "IDCompany");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Companies_IDCompany",
                table: "Employees",
                column: "IDCompany",
                principalTable: "Companies",
                principalColumn: "IDCompany",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
