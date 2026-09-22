using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceParkingEntriesWithParkingTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingEntries");

            migrationBuilder.CreateTable(
                name: "ParkingTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParkingSpotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    EnteredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EnteredByEmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExitedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExitedByEmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingTickets_AspNetUsers_EnteredByEmployeeId",
                        column: x => x.EnteredByEmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkingTickets_AspNetUsers_ExitedByEmployeeId",
                        column: x => x.ExitedByEmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkingTickets_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkingTickets_ParkingSpots_ParkingSpotId",
                        column: x => x.ParkingSpotId,
                        principalTable: "ParkingSpots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_ActivePerPlate",
                table: "ParkingTickets",
                columns: new[] { "CompanyId", "LicensePlate" },
                unique: true,
                filter: "[ExitedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_ActivePerSpot",
                table: "ParkingTickets",
                column: "ParkingSpotId",
                unique: true,
                filter: "[ExitedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_BranchId",
                table: "ParkingTickets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_CompanyId",
                table: "ParkingTickets",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_EnteredByEmployeeId",
                table: "ParkingTickets",
                column: "EnteredByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingTickets_ExitedByEmployeeId",
                table: "ParkingTickets",
                column: "ExitedByEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkingTickets");

            migrationBuilder.CreateTable(
                name: "ParkingEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Car = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntryDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExitDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ParkingPositionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisteredByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingEntries", x => x.Id);
                });
        }
    }
}
