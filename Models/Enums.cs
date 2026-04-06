namespace ParkingSystem.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public enum PaymentStatus
    {
        Pending,
        Successful,
        Failed,
        Refunded
    }

    public enum SlotStatus
    {
        Available,
        Reserved,
        Occupied
    }
}