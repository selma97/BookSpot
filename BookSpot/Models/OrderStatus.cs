using System.ComponentModel.DataAnnotations;

namespace BookSpot.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }
        [Required]
        public int StatusId { get; set; }

        [Required, MaxLength(20)]
        public string StatusName { get; set; }  = string.Empty;
    }
}
