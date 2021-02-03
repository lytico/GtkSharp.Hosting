using Microsoft.EntityFrameworkCore;
using MyApp.Entity;

namespace MyApp.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Blog> Blogs { get; set; }
    }
}