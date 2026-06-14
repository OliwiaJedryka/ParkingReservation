using System.ComponentModel.DataAnnotations;

namespace ParkingReservation.Models
{
    public class ParkingSpot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SpotNumber { get; set; } = string.Empty; // np. "A-01"

        public bool IsOccupied { get; set; } = false;
    }
}