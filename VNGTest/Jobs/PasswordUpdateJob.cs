using Microsoft.EntityFrameworkCore;
using VNGTest.Models;
using VNGTest.Services;

namespace VNGTest.Jobs
{
    public class PasswordUpdateJob
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;

        public PasswordUpdateJob(ApplicationDbContext dbContext, IEmailService emailService)
        {
            _dbContext = dbContext;
            _emailService = emailService;
        }

        public async Task ExecuteAsync()
        {
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var usersToUpdate = await _dbContext.Users
                .Where(u => u.LastUpdatePwd < sixMonthsAgo && u.Status != "REQUIRE_CHANGE_PWD")
                .ToListAsync();

            foreach (var user in usersToUpdate)
            {
                user.Status = "REQUIRE_CHANGE_PWD";
                await _emailService.SendEmailAsync(user.Email, "Password Change Required", "Please change your password.");
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
