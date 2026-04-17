using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddParkingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_AspNetUsers_UserId",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingSlot_Booking_BookingId",
                table: "BookingSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingSlot_ParkingSlot_SlotId",
                table: "BookingSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Booking_BookingId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlot_ParkingArea_AreaId",
                table: "ParkingSlot");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Booking_BookingId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_AspNetUsers_UserId",
                table: "Vehicle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vehicle",
                table: "Vehicle");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSlot",
                table: "ParkingSlot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingArea",
                table: "ParkingArea");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingSlot",
                table: "BookingSlot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.RenameTable(
                name: "Vehicle",
                newName: "Vehicles");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "ParkingSlot",
                newName: "ParkingSlots");

            migrationBuilder.RenameTable(
                name: "ParkingArea",
                newName: "ParkingAreas");

            migrationBuilder.RenameTable(
                name: "BookingSlot",
                newName: "BookingSlots");

            migrationBuilder.RenameTable(
                name: "Booking",
                newName: "Bookings");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicle_UserId",
                table: "Vehicles",
                newName: "IX_Vehicles_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_BookingId",
                table: "Payments",
                newName: "IX_Payments_BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSlot_AreaId",
                table: "ParkingSlots",
                newName: "IX_ParkingSlots_AreaId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingSlot_SlotId",
                table: "BookingSlots",
                newName: "IX_BookingSlots_SlotId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingSlot_BookingId",
                table: "BookingSlots",
                newName: "IX_BookingSlots_BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_UserId",
                table: "Bookings",
                newName: "IX_Bookings_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles",
                column: "VehicleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "PaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSlots",
                table: "ParkingSlots",
                column: "SlotId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingAreas",
                table: "ParkingAreas",
                column: "AreaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingSlots",
                table: "BookingSlots",
                column: "BookingSlotId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSlots_Bookings_BookingId",
                table: "BookingSlots",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSlots_ParkingSlots_SlotId",
                table: "BookingSlots",
                column: "SlotId",
                principalTable: "ParkingSlots",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Bookings_BookingId",
                table: "Invoice",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_AreaId",
                table: "ParkingSlots",
                column: "AreaId",
                principalTable: "ParkingAreas",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_AspNetUsers_UserId",
                table: "Vehicles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_UserId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingSlots_Bookings_BookingId",
                table: "BookingSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingSlots_ParkingSlots_SlotId",
                table: "BookingSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_Bookings_BookingId",
                table: "Invoice");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingSlots_ParkingAreas_AreaId",
                table: "ParkingSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_AspNetUsers_UserId",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vehicles",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingSlots",
                table: "ParkingSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkingAreas",
                table: "ParkingAreas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookingSlots",
                table: "BookingSlots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "Vehicles",
                newName: "Vehicle");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameTable(
                name: "ParkingSlots",
                newName: "ParkingSlot");

            migrationBuilder.RenameTable(
                name: "ParkingAreas",
                newName: "ParkingArea");

            migrationBuilder.RenameTable(
                name: "BookingSlots",
                newName: "BookingSlot");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "Booking");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_UserId",
                table: "Vehicle",
                newName: "IX_Vehicle_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_BookingId",
                table: "Payment",
                newName: "IX_Payment_BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_ParkingSlots_AreaId",
                table: "ParkingSlot",
                newName: "IX_ParkingSlot_AreaId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingSlots_SlotId",
                table: "BookingSlot",
                newName: "IX_BookingSlot_SlotId");

            migrationBuilder.RenameIndex(
                name: "IX_BookingSlots_BookingId",
                table: "BookingSlot",
                newName: "IX_BookingSlot_BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_UserId",
                table: "Booking",
                newName: "IX_Booking_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vehicle",
                table: "Vehicle",
                column: "VehicleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "PaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingSlot",
                table: "ParkingSlot",
                column: "SlotId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkingArea",
                table: "ParkingArea",
                column: "AreaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookingSlot",
                table: "BookingSlot",
                column: "BookingSlotId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booking",
                table: "Booking",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_AspNetUsers_UserId",
                table: "Booking",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSlot_Booking_BookingId",
                table: "BookingSlot",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingSlot_ParkingSlot_SlotId",
                table: "BookingSlot",
                column: "SlotId",
                principalTable: "ParkingSlot",
                principalColumn: "SlotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_Booking_BookingId",
                table: "Invoice",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingSlot_ParkingArea_AreaId",
                table: "ParkingSlot",
                column: "AreaId",
                principalTable: "ParkingArea",
                principalColumn: "AreaId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Booking_BookingId",
                table: "Payment",
                column: "BookingId",
                principalTable: "Booking",
                principalColumn: "BookingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_AspNetUsers_UserId",
                table: "Vehicle",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
