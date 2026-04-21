using System.ComponentModel.DataAnnotations;

public class TempBooking
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; }

    public string Slots { get; set; }

    public int Duration { get; set; }

    public decimal Total { get; set; }

    public string TransactionId { get; set; }
    public DateTime BookingDate { get; set; }
}