using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class slotrelated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ParkingAreas");

            migrationBuilder.AlterColumn<string>(
                name: "BlockNumber",
                table: "ParkingAreas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "VehicleType",
                table: "ParkingAreas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingAreas_BlockNumber_VehicleType",
                table: "ParkingAreas",
                columns: new[] { "BlockNumber", "VehicleType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ParkingAreas_BlockNumber_VehicleType",
                table: "ParkingAreas");

            migrationBuilder.DropColumn(
                name: "VehicleType",
                table: "ParkingAreas");

            migrationBuilder.AlterColumn<string>(
                name: "BlockNumber",
                table: "ParkingAreas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ParkingAreas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
