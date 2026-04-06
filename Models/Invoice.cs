using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public decimal TotalAmount { get; set; }

        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}