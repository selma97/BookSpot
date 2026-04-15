using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSpot.Models
{
    public class Book
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(40)]
        public string BookName { get; set; } = string.Empty;
        [Required]
        public string AuthorName { get; set; } = string.Empty;
        [Required]
        public double Price { get; set; }
        [Required]
        public int GenreId { get; set; }
        public Genre Genre { get; set; }   

        public string? Image { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }

        public List<CartDetail> CartDetails { get; set; }

        [NotMapped]
        public string GenreName { get; set; }

    }
}
