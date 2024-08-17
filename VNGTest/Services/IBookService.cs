using VNGTest.Models;

namespace VNGTest.Services
{
    public interface IBookService
    {
        IEnumerable<Book> GetBooks();
        Book GetBook(int id);
        Book AddBook(Book book);
        bool UpdateBook(int id, Book book);
        bool DeleteBook(int id);
    }
}
