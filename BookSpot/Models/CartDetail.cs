using System.ComponentModel.DataAnnotations;

namespace BookSpot.Models
{
    public class CartDetail
    {
        public int Id { get; set; }
        [Required]
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
        [Required]
        public int BookId { get; set; }
        public Book Book { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
