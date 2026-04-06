using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public string TransactionId { get; set; }

        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }
    }
}