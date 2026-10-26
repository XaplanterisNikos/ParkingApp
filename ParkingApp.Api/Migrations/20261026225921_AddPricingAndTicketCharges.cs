using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingAndTicketCharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountCharged",
                table: "ParkingTickets",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OvernightPackageId",
                table: "ParkingTickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TariffVersionId",
                table: "ParkingTickets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatRate",
                table: "ParkingTickets",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SundaysAreHolidays",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TariffVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    StepMinutes = table.Column<int>(type: "int", nullable: false),
                    GraceMinutes = table.Column<int>(type: "int", nullable: false),
                    NightStart = table.Column<TimeOnly>(type: "time", nullable: false),
                    NightEnd = table.Column<TimeOnly>(type: "time", nullable: false),
                    OvernightCheckoutTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TariffVersions_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OvernightPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TariffVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Nights = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvernightPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvernightPackages_TariffVersions_TariffVersionId",
                        column: x => x.TariffVersionId,
                        principalTable: "TariffVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TariffRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TariffVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    DayType = table.Column<int>(type: "int", nullable: false),
                    DayStepPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    NightStepPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DailyCap = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TariffRates_TariffVersions_TariffVersionId",
                        column: x => x.TariffVersionId,
                        principalTable: "TariffVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_OvernightPackageId",
                table: "ParkingTickets",
                column: "OvernightPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_TariffVersionId",
                table: "ParkingTickets",
                column: "TariffVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CompanyId_Date",
                table: "Holidays",
                columns: new[] { "CompanyId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OvernightPackages_CompanyId",
                table: "OvernightPackages",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_OvernightPackages_TariffVersionId",
                table: "OvernightPackages",
                column: "TariffVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_TariffRates_CompanyId",
                table: "TariffRates",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TariffRates_TariffVersionId_Size_DayType",
                table: "TariffRates",
                columns: new[] { "TariffVersionId", "Size", "DayType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TariffVersions_BranchId_CreatedAt",
                table: "TariffVersions",
                columns: new[] { "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TariffVersions_CompanyId",
                table: "TariffVersions",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingTickets_OvernightPackages_OvernightPackageId",
                table: "ParkingTickets",
                column: "OvernightPackageId",
                principalTable: "OvernightPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingTickets_TariffVersions_TariffVersionId",
                table: "ParkingTickets",
                column: "TariffVersionId",
                principalTable: "TariffVersions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingTickets_OvernightPackages_OvernightPackageId",
                table: "ParkingTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingTickets_TariffVersions_TariffVersionId",
                table: "ParkingTickets");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "OvernightPackages");

            migrationBuilder.DropTable(
                name: "TariffRates");

            migrationBuilder.DropTable(
                name: "TariffVersions");

            migrationBuilder.DropIndex(
                name: "IX_ParkingTickets_OvernightPackageId",
                table: "ParkingTickets");

            migrationBuilder.DropIndex(
                name: "IX_ParkingTickets_TariffVersionId",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "AmountCharged",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "OvernightPackageId",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "TariffVersionId",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "SundaysAreHolidays",
                table: "Companies");
        }
    }
}
