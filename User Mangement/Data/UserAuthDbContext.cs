using Microsoft.EntityFrameworkCore;
using User_Mangement.Models;

namespace User_Mangement.Data
{
    public class UserAuthDbContext :DbContext
    {

         public UserAuthDbContext(DbContextOptions<UserAuthDbContext> dbContext) : base(dbContext) { }

        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<PasswordDetails> PasswordDetails { get; set; }
        public DbSet<VaildationToken> VaildationTokens { get; set; }
    }
}
