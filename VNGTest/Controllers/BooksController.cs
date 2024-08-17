using Microsoft.AspNetCore.Mvc;
using VNGTest.Models;
using VNGTest.Services;

namespace BookApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        private bool IsAuthorized()
        {
            var authHeader = Request.Headers["xAuth"].ToString();
            return !string.IsNullOrEmpty(authHeader);
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            if (!IsAuthorized())
                return Unauthorized(new ApiResponse<IEnumerable<Book>>(false, "Unauthorized", null, 401));

            var books = _bookService.GetBooks();
            return Ok(new ApiResponse<IEnumerable<Book>>(true, "Books retrieved successfully", books, 200));
        }

        [HttpGet("{id}")]
        public IActionResult GetBook(int id)
        {
            if (!IsAuthorized())
                return Unauthorized(new ApiResponse<Book>(false, "Unauthorized", null, 401));

            var book = _bookService.GetBook(id);
            if (book == null)
                return NotFound(new ApiResponse<Book>(false, "Book not found", null, 404));

            return Ok(new ApiResponse<Book>(true, "Book retrieved successfully", book, 200));
        }

        [HttpPost]
        public IActionResult CreateBook(Book book)
        {
            if (!IsAuthorized())
                return Unauthorized(new ApiResponse<Book>(false, "Unauthorized", null, 401));

            var createdBook = _bookService.AddBook(book);
            return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id },
                new ApiResponse<Book>(true, "Book created successfully", createdBook, 201));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBook(int id, Book book)
        {
            if (!IsAuthorized())
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null, 401));

            var updated = _bookService.UpdateBook(id, book);
            if (!updated)
                return NotFound(new ApiResponse<object>(false, "Book not found", null, 404));

            // You can use `Ok` here for successful update
            return Ok(new ApiResponse<object>(true, "Book updated successfully", null, 200));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            if (!IsAuthorized())
                return Unauthorized(new ApiResponse<object>(false, "Unauthorized", null, 401));

            var deleted = _bookService.DeleteBook(id);
            if (!deleted)
                return NotFound(new ApiResponse<object>(false, "Book not found", null, 404));

            // You can use `Ok` here for successful deletion
            return Ok(new ApiResponse<object>(true, "Book deleted successfully", null, 200));
        }
    }
}
