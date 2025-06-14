using Microsoft.EntityFrameworkCore;

namespace PlayerAPI.Models
{
    public class FileContext : DbContext
    {
        public virtual DbSet<PlayList> PlayLists { get; set; }

        public virtual DbSet<File> File { get; set; }

        private readonly IConfiguration _configuration;

        public FileContext(IConfiguration configuration)
        {
            _configuration = configuration;
            this.Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Postgres"));
        }
    }
}
