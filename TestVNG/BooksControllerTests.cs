using BookApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using VNGTest.Models;
using VNGTest.Services;
using Xunit;

namespace BookApi.Tests
{
    public class BooksControllerTests
    {
        private readonly BooksController _controller;
        private readonly Mock<IBookService> _mockBookService;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BooksController(_mockBookService.Object);
        }

        private void SetAuthorizationHeader(string value)
        {
            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
            _controller.Request.Headers["xAuth"] = value;
        }

        [Fact]
        public void GetBooks_ReturnsUnauthorized_WhenNoAuthHeader()
        {
            // Arrange
            SetAuthorizationHeader("");

            // Act
            var result = _controller.GetBooks() as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<IEnumerable<Book>>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(401, response.StatusCode);
        }

        [Fact]
        public void GetBooks_ReturnsOk_WhenAuthorized()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            var books = new List<Book> { new Book { Id = 1, Title = "Book1", Author = "Author1", PublishedYear = 2024 } };
            _mockBookService.Setup(service => service.GetBooks()).Returns(books);

            // Act
            var result = _controller.GetBooks() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<IEnumerable<Book>>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal(books, response.Data);
        }

        [Fact]
        public void GetBooks_ReturnsEmptyList_WhenNoBooksAvailable()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            var books = new List<Book>();
            _mockBookService.Setup(service => service.GetBooks()).Returns(books);

            // Act
            var result = _controller.GetBooks() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<IEnumerable<Book>>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
            Assert.Empty(response.Data);
        }

        [Fact]
        public void GetBook_ReturnsNotFound_WhenBookNotExists()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            _mockBookService.Setup(service => service.GetBook(1)).Returns((Book)null);

            // Act
            var result = _controller.GetBook(1) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<Book>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public void CreateBook_ReturnsCreatedAtAction_WhenAuthorized()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            var book = new Book { Id = 1, Title = "New Book", Author = "New Author", PublishedYear = 2024 };
            _mockBookService.Setup(service => service.AddBook(It.IsAny<Book>())).Returns(book);

            // Act
            var result = _controller.CreateBook(book) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<Book>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(201, response.StatusCode);
            Assert.Equal(book, response.Data);
        }

        [Fact]
        public void UpdateBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            var book = new Book { Id = 1, Title = "Updated Book", Author = "Updated Author", PublishedYear = 2024 };
            _mockBookService.Setup(service => service.UpdateBook(1, book)).Returns(false);

            // Act
            var result = _controller.UpdateBook(1, book) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<object>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public void UpdateBook_ReturnsOk_WhenBookUpdatedSuccessfully()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            var book = new Book { Id = 1, Title = "Updated Book", Author = "Updated Author", PublishedYear = 2024 };
            _mockBookService.Setup(service => service.UpdateBook(1, book)).Returns(true);

            // Act
            var result = _controller.UpdateBook(1, book) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<object>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
        }

        [Fact]
        public void DeleteBook_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            _mockBookService.Setup(service => service.DeleteBook(1)).Returns(false);

            // Act
            var result = _controller.DeleteBook(1) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<object>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal(404, response.StatusCode);
        }

        [Fact]
        public void DeleteBook_ReturnsOk_WhenSuccessfullyDeleted()
        {
            // Arrange
            SetAuthorizationHeader("valid-token");
            _mockBookService.Setup(service => service.DeleteBook(1)).Returns(true);

            // Act
            var result = _controller.DeleteBook(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var response = result.Value as ApiResponse<object>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(200, response.StatusCode);
        }
    }
}
