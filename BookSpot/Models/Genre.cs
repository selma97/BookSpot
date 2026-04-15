namespace BookSpot.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string GenreName { get; set; }

        public List<Book> Books { get; set; }
    }
}
