using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class FloorTypeRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Floors_BranchId",
                table: "Floors");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Floors");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Floors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Floors_BranchId_Type",
                table: "Floors",
                columns: new[] { "BranchId", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Floors_BranchId_Type",
                table: "Floors");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Floors");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Floors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Floors_BranchId",
                table: "Floors",
                column: "BranchId");
        }
    }
}
