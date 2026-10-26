using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlateKindAndPerBranchPlateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParkingTickets_ActivePerPlate",
                table: "ParkingTickets");

            migrationBuilder.DropIndex(
                name: "IX_ParkingTickets_BranchId",
                table: "ParkingTickets");

            migrationBuilder.AddColumn<int>(
                name: "PlateKind",
                table: "ParkingTickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_ActivePerPlate",
                table: "ParkingTickets",
                columns: new[] { "BranchId", "LicensePlate" },
                unique: true,
                filter: "[ExitedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParkingTickets_ActivePerPlate",
                table: "ParkingTickets");

            migrationBuilder.DropColumn(
                name: "PlateKind",
                table: "ParkingTickets");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_ActivePerPlate",
                table: "ParkingTickets",
                columns: new[] { "CompanyId", "LicensePlate" },
                unique: true,
                filter: "[ExitedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_BranchId",
                table: "ParkingTickets",
                column: "BranchId");
        }
    }
}
