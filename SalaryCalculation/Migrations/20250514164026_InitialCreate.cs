using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalaryCalculation.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryCalculations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSalaryGross = table.Column<bool>(type: "bit", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: false),
                    ShowEmployerCost = table.Column<bool>(type: "bit", nullable: false),
                    CalculationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryCalculations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    SgkEmployeeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SgkEmployerRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnemploymentEmployeeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnemploymentEmployerRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StampTaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket1Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket1Limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket2Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket2Limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket3Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket3Limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket4Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTaxBracket5Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumWageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalaryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Month = table.Column<int>(type: "int", nullable: false),
                    MonthName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GrossSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SgkEmployeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnemploymentEmployeeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StampTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CumulativeIncomeTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumWageTaxDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SgkEmployerAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnemploymentEmployerAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEmployerCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalaryCalculationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryDetails_SalaryCalculations_SalaryCalculationId",
                        column: x => x.SalaryCalculationId,
                        principalTable: "SalaryCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDetails_SalaryCalculationId",
                table: "SalaryDetails",
                column: "SalaryCalculationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryDetails");

            migrationBuilder.DropTable(
                name: "TaxRates");

            migrationBuilder.DropTable(
                name: "SalaryCalculations");
        }
    }
}
