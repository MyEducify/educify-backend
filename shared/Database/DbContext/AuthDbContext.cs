using Microsoft.EntityFrameworkCore;

namespace Database.DbContext
{
    public class AuthDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
    }

}
