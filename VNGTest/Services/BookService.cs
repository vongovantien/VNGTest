using VNGTest.Models;

namespace VNGTest.Services
{
    public class BookService : IBookService
    {
        private readonly List<Book> _books = new List<Book>
        {
            new Book { Id = 1, Title = "Harper Lee", Author = "George Orwell", PublishedYear = 1949 },
            new Book { Id = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", PublishedYear = 1960 }
        };

        public IEnumerable<Book> GetBooks() => _books;

        public Book GetBook(int id) => _books.FirstOrDefault(b => b.Id == id);

        public Book AddBook(Book book)
        {
            _books.Add(book);
            return book;
        }

        public bool UpdateBook(int id, Book book)
        {
            var existingBook = GetBook(id);
            if (existingBook == null) return false;
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.PublishedYear = book.PublishedYear;
            return true;
        }

        public bool DeleteBook(int id)
        {
            var book = GetBook(id);
            if (book == null) return false;
            _books.Remove(book);
            return true;
        }
    }
}
