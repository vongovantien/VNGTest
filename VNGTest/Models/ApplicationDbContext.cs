using Microsoft.EntityFrameworkCore;

namespace VNGTest.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(256);
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(e => e.LastUpdatePwd)
                      .IsRequired();
            });
        }
    }
}
