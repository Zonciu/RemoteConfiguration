using Microsoft.EntityFrameworkCore;

namespace RemoteDb.Basic
{
    public class MyDb : DbContext
    {
        public DbSet<MyOptionsModel> Options { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Options");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MyOptionsModel>(
                builder =>
                {
                    builder.HasKey(e => e.Key);
                    builder.Property(e => e.Value);
                });
            base.OnModelCreating(modelBuilder);
        }
    }
}