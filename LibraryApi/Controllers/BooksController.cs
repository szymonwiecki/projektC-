using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;


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
        public ActionResult<Book> CreateBook(AddBook newBook)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingBook = _context.Books
                .FirstOrDefault(b => b.Title.ToLower() == newBook.Title.ToLower()
                                && b.Author.ToLower() == newBook.Author.ToLower());

            if (existingBook != null)
            {
                return Conflict(new
                {
                    message = "Book already exists",
                    existingBookId = existingBook.Id,
                    details = "A book with the same title and author already exists in the database"
                });
            }

            try
            {
                var book = new Book
                {
                    Title = newBook.Title,
                    Author = newBook.Author,
                    PublishedYear = newBook.PublishedYear,
                    Genre = newBook.Genre
                };

                _context.Books.Add(book);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
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

                // Sprawdzenie czy nowe dane nie koliduj¹ z inn¹ ksi¹¿k¹
                var duplicateBook = _context.Books
                    .FirstOrDefault(b => b.Id != id
                                     && b.Title.ToLower() == book.Title.ToLower()
                                     && b.Author.ToLower() == book.Author.ToLower());

                if (duplicateBook != null)
                {
                    return Conflict(new
                    {
                        message = "Update would create duplicate book",
                        existingBookId = duplicateBook.Id,
                        details = "Another book with the same title and author already exists"
                    });
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

        [Authorize]
        [HttpGet("export")]
        public async Task<IActionResult> ExportBooks()
        {
            var books = await _context.Books.ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Id,Title,Author,PublishedYear,Genre");

            foreach (var book in books)
            {
                csv.AppendLine($"{book.Id},{book.Title},{book.Author},{book.PublishedYear},{book.Genre}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "books_export.csv");
        }

        [Authorize]    
        [HttpPost("import")]
        public async Task<IActionResult> ImportBooks(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var importedBooks = new List<Book>();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string? headerLine = await reader.ReadLineAsync(); // Skip header
                if (headerLine == null || !headerLine.StartsWith("Title"))
                    return BadRequest("Invalid file format. Header must start with 'Title'");

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length != 4) continue;

                    try
                    {
                        var book = new Book
                        {
                            Title = parts[0],
                            Author = parts[1],
                            PublishedYear = int.Parse(parts[2]),
                            Genre = parts[3]
                        };
                        importedBooks.Add(book);
                    }
                    catch
                    {
                        // loguj b³êdne linie
                        continue;
                    }
                }
            }

            _context.Books.AddRange(importedBooks);
            await _context.SaveChangesAsync();

            return Ok(new { count = importedBooks.Count, message = "Books imported successfully" });
        }


    }


}
