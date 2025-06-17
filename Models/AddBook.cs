using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models
{
    public class AddBook
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required")]
        [StringLength(100, ErrorMessage = "Author name cannot be longer than 100 characters")]
        public string Author { get; set; }

        [Range(1, 9999, ErrorMessage = "Published year must be between 1 and 9999")]
        public int PublishedYear { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        [StringLength(50, ErrorMessage = "Genre cannot be longer than 50 characters")]
        public string Genre { get; set; }
    }
}

