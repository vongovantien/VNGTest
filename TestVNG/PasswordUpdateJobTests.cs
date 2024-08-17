using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VNGTest.Jobs;
using VNGTest.Models;
using VNGTest.Services;
using Xunit;

namespace VNGTest.Tests
{
    public class PasswordUpdateJobTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly PasswordUpdateJob _job;

        public PasswordUpdateJobTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _emailServiceMock = new Mock<IEmailService>();
            _job = new PasswordUpdateJob(_dbContext, _emailServiceMock.Object);
        }

        private void SeedDatabase(IEnumerable<User> users)
        {
            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task ExecuteAsync_UpdatesStatusAndSendsEmail()
        {
            // Arrange
            SeedDatabase(new List<User>
            {
                new User { Id = 3, Email = "user1@example.com", LastUpdatePwd = DateTime.Now.AddMonths(-7), Status = "ACTIVE" },
                new User { Id = 5, Email = "user2@example.com", LastUpdatePwd = DateTime.Now.AddMonths(-5), Status = "ACTIVE" }
            });

            // Act
            await _job.ExecuteAsync();

            // Assert
            var updatedUser = _dbContext.Users.Single(u => u.Id == 3);
            Assert.Equal("REQUIRE_CHANGE_PWD", updatedUser.Status);
            _emailServiceMock.Verify(s => s.SendEmailAsync("user1@example.com", "Password Change Required", "Please change your password."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotUpdateStatus_IfAlreadyRequiresChange()
        {
            // Arrange
            SeedDatabase(new List<User>
            {
                new User { Id = 16, Email = "user1@example.com", LastUpdatePwd = DateTime.Now.AddMonths(-7), Status = "REQUIRE_CHANGE_PWD" }
            });

            // Act
            await _job.ExecuteAsync();

            // Assert
            var updatedUser = _dbContext.Users.Single(u => u.Id == 1);
            Assert.Equal("REQUIRE_CHANGE_PWD", updatedUser.Status);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

 
        [Fact]
        public async Task ExecuteAsync_SendsEmailForEachUserRequiringChange()
        {
            // Arrange
            SeedDatabase(new List<User>
            {
                new User { Id = 12, Email = "user1@example.com", LastUpdatePwd = DateTime.Now.AddMonths(-7), Status = "ACTIVE" },
                new User { Id = 23, Email = "user2@example.com", LastUpdatePwd = DateTime.Now.AddMonths(-7), Status = "ACTIVE" }
            });

            // Act
            await _job.ExecuteAsync();

            // Assert
            _emailServiceMock.Verify(s => s.SendEmailAsync("user1@example.com", "Password Change Required", "Please change your password."), Times.Once);
            _emailServiceMock.Verify(s => s.SendEmailAsync("user2@example.com", "Password Change Required", "Please change your password."), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotThrowExceptionForEmptyDatabase()
        {
            // Arrange
            SeedDatabase(new List<User>());

            // Act & Assert
            await _job.ExecuteAsync();
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotSendEmailIfNoUsersNeedUpdate()
        {
            // Arrange
            SeedDatabase(new List<User>
            {
                new User { Id = 4, Email = "user1@example.com", LastUpdatePwd = DateTime.Now.AddMonths(-4), Status = "ACTIVE" }
            });

            // Act
            await _job.ExecuteAsync();

            // Assert
            var updatedUser = _dbContext.Users.Single(u => u.Id == 4);
            Assert.Equal("ACTIVE", updatedUser.Status);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_CorrectlyUpdatesUserWithStatusAlreadyChanged()
        {
            // Arrange
            SeedDatabase(new List<User>
            {
                new User { Id = 1, Email = "user1@example.com", LastUpdatePwd = DateTime.Now.AddMonths(-7), Status = "REQUIRE_CHANGE_PWD" }
            });

            // Act
            await _job.ExecuteAsync();

            // Assert
            var updatedUser = _dbContext.Users.Single(u => u.Id == 1);
            Assert.Equal("REQUIRE_CHANGE_PWD", updatedUser.Status);
            _emailServiceMock.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
