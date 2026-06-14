using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingReservation.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        public int ParkingSpotId { get; set; }
        
        [ForeignKey("ParkingSpotId")]
        public ParkingSpot? ParkingSpot { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}