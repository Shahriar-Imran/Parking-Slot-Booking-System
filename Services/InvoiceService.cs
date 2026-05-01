using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ParkingSystem.Models;
using System.Data.SqlTypes;

namespace ParkingSystem.Services
{
    public class InvoiceService
    {
        public byte[] GenerateInvoice(Booking booking, ApplicationUser user, byte[] qrBytes)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    // 🔹 CARD STYLE
                    page.Content().Padding(20).Border(1).Column(col =>
                    {
                        // 🔥 TITLE
                        col.Item().AlignCenter().Text("Booking Invoice")
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Green.Medium);

                        col.Item().LineHorizontal(1);

                        // 🔹 BOOKING INFO
                        col.Item().Text($"Booking ID: {booking.BookingId}").Bold();
                        col.Item().Text($"Transaction ID: {booking.TransactionId}");
                        col.Item().Text($"User Name: {user?.FullName}");

                        col.Item().LineHorizontal(1);

                        // 🔹 SLOT DETAILS
                        col.Item().Text("Slot Details").Bold().FontSize(14);

                        foreach (var bs in booking.BookingSlots)
                        {
                            col.Item().Text(
                                $"• Slot {bs.Slot.SlotNumber} - {bs.Slot.ParkingArea.VehicleType}");
                        }

                        col.Item().LineHorizontal(1);

                        // 🔹 TIME + AMOUNT
                        col.Item().Text($"Start Time: {booking.StartTime}");
                        col.Item().Text($"End Time: {booking.EndTime}");
                        col.Item().Text($"Total Amount: ৳ {booking.TotalAmount}")
                            .Bold()
                            .FontColor(Colors.Green.Darken2);

                        col.Item().LineHorizontal(1);

                        // 🔹 QR CODE
                        col.Item().AlignCenter().Column(qrCol =>
                        {
                            qrCol.Item().Text("QR Code").Bold();

                            qrCol.Item()
                                .Width(120)
                                .Height(120)
                                .Image(qrBytes);
                        });
                    });

                    // 🔻 FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for using Parking System")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}
