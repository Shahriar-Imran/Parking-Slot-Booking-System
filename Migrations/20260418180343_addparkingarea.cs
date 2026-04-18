using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class addparkingarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_AreaId",
                table: "ParkingSlots");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSlots_AreaId",
                table: "ParkingSlots");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "ParkingSlots");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ParkingAreas");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "ParkingAreas");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ParkingSlots",
                newName: "ParkingAreaAreaId");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "ParkingAreas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSlots_ParkingAreaAreaId",
                table: "ParkingSlots",
                column: "ParkingAreaAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_ParkingAreaAreaId",
                table: "ParkingSlots",
                column: "ParkingAreaAreaId",
                principalTable: "ParkingAreas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_ParkingAreaAreaId",
                table: "ParkingSlots");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSlots_ParkingAreaAreaId",
                table: "ParkingSlots");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "ParkingAreas");

            migrationBuilder.RenameColumn(
                name: "ParkingAreaAreaId",
                table: "ParkingSlots",
                newName: "Status");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "ParkingSlots",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ParkingAreas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "ParkingAreas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSlots_AreaId",
                table: "ParkingSlots",
                column: "AreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_AreaId",
                table: "ParkingSlots",
                column: "AreaId",
                principalTable: "ParkingAreas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
