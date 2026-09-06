using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyCodeAndEmployeeBranches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Companies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EmployeeBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeBranches_AspNetUsers_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeBranches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Code",
                table: "Companies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBranches_BranchId",
                table: "EmployeeBranches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBranches_CompanyId",
                table: "EmployeeBranches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBranches_EmployeeId_BranchId",
                table: "EmployeeBranches",
                columns: new[] { "EmployeeId", "BranchId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeBranches");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Code",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Companies");
        }
    }
}
