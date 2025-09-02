using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.DBContext
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> opts) : base(opts) { }
        public DbSet<User> user => Set<User>();
    }
}
