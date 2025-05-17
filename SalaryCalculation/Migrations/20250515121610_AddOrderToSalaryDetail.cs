using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalaryCalculation.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderToSalaryDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "SalaryDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "SalaryDetails");
        }
    }
}
