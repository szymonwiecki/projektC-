using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;


namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BooksController(LibraryContext context)
        {
            _context = context;
        }

        // Pobranie listy ksi��ek
        [Authorize]
        [HttpGet]
        [SwaggerOperation(Summary = "Show book list", Description = "This endpoint will show you books that are on the list.")]
        public ActionResult<IEnumerable<Book>> GetBooks()
        {
            try
            {
                return Ok(_context.Books.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching books", details = ex.Message });
            }
        }

        // Pobranie jednej ksi��ki
        [Authorize]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Show selected book", Description = "This endpoint allows will show you selected (by id) book.")]
        public ActionResult<Book> GetBook(int id)
        {
            var book = _context.Books.Find(id);

            if (book == null)
                return NotFound(new { message = "Book not found" });

            return Ok(book);
        }
        // Dodanie nowej ksi��ki
        [Authorize]
        [HttpPost]
        [SwaggerOperation(Summary = "Add a new book", Description = "This endpoint allows you to add a new book to the library.")]
        public ActionResult<Book> CreateBook(Book book)
        {
            try
            {
                _context.Books.Add(book);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the book", details = ex.Message });
            }
        }

        // Aktualizacja istniej�cej ksi��ki
        [Authorize]
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Chenge existing book", Description = "This endpoint allows you change data in existing book.")]
        public ActionResult UpdateBook(int id, Book book)
        {
            if (id != book.Id)
                return BadRequest(new { message = "ID in URL and body must match" });

            var existingBook = _context.Books.Find(id);
            if (existingBook == null)
                return NotFound(new { message = "Book not found" });

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.PublishedYear = book.PublishedYear;
            existingBook.Genre = book.Genre;

            _context.SaveChanges();
            return NoContent();
        }

        // Usuni�cie ksi��ki
        [Authorize]
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Remove a book", Description = "This endpoint allows you to remove a book from library.")]
        public ActionResult DeleteBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
                return NotFound(new { message = "Book not found" });

            _context.Books.Remove(book);
            _context.SaveChanges();
            return NoContent();
        }

    }


}
