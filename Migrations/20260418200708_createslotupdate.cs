using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class createslotupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_ParkingAreaAreaId",
                table: "ParkingSlots");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSlots_ParkingAreaAreaId",
                table: "ParkingSlots");

            migrationBuilder.DropColumn(
                name: "ParkingAreaAreaId",
                table: "ParkingSlots");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_AreaId",
                table: "ParkingSlots");

            migrationBuilder.DropIndex(
                name: "IX_ParkingSlots_AreaId",
                table: "ParkingSlots");

            migrationBuilder.AddColumn<int>(
                name: "ParkingAreaAreaId",
                table: "ParkingSlots",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
    }
}
