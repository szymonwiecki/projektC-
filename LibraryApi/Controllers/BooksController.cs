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

        // Pobranie listy ksi¹¿ek
        [Authorize]
        [HttpGet]
        [SwaggerOperation(Summary = "Show book list", Description = "This endpoint will show you books that are on the list.")]
        public ActionResult<IEnumerable<Book>> GetBooks()
        {
            try
            {
                var books = _context.Books.ToList();
                return Ok(books);
            }
            catch (Exception ex)
            {
                // Logujemy b³¹d (w rzeczywistoœci u¿yj np. logowania do pliku lub systemu logowania)
                return StatusCode(500, new { message = "An error occurred while fetching books", details = ex.Message });
            }
        }

        // Pobranie jednej ksi¹¿ki
        [Authorize]
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Show selected book", Description = "This endpoint allows will show you selected (by id) book.")]
        public ActionResult<Book> GetBook(int id)
        {
            try
            {
                var book = _context.Books.Find(id);
                if (book == null)
                    return NotFound(new { message = "Book not found" });

                return Ok(book);
            }
            catch (Exception ex)
            {
                // Logowanie b³êdu
                return StatusCode(500, new { message = "An error occurred while fetching the book", details = ex.Message });
            }
        }
        // Dodanie nowej ksi¹¿ki
        [Authorize]
        [HttpPost]
        [SwaggerOperation(Summary = "Add a new book", Description = "This endpoint allows you to add a new book to the library.")]
        public ActionResult<Book> CreateBook(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Zwracamy komunikat o b³êdzie, jeœli model nie jest poprawny
            }

            try
            {
                _context.Books.Add(book);
                _context.SaveChanges();
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                // Logowanie b³êdu
                return StatusCode(500, new { message = "An error occurred while creating the book", details = ex.Message });
            }
        }

        // Aktualizacja istniej¹cej ksi¹¿ki
        [Authorize]
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Chenge existing book", Description = "This endpoint allows you change data in existing book.")]
        public ActionResult UpdateBook(int id, Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Zwracamy komunikat o b³êdzie, jeœli model nie jest poprawny
            }

            if (id != book.Id)
            {
                return BadRequest(new { message = "ID in URL and body must match" });
            }

            try
            {
                var existingBook = _context.Books.Find(id);
                if (existingBook == null)
                {
                    return NotFound(new { message = "Book not found" });
                }

                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.PublishedYear = book.PublishedYear;
                existingBook.Genre = book.Genre;

                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Logowanie b³êdu
                return StatusCode(500, new { message = "An error occurred while updating the book", details = ex.Message });
            }
        }

        // Usuniêcie ksi¹¿ki
        [Authorize]
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Remove a book", Description = "This endpoint allows you to remove a book from library.")]
        public ActionResult DeleteBook(int id)
        {
            try
            {
                var book = _context.Books.Find(id);
                if (book == null)
                    return NotFound(new { message = "Book not found" });

                _context.Books.Remove(book);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Logowanie b³êdu
                return StatusCode(500, new { message = "An error occurred while deleting the book", details = ex.Message });
            }
        }

    }


}
