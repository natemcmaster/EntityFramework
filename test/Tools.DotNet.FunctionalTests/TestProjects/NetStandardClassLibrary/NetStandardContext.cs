using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace NetStandardClassLibrary
{
    public class NetStandardContext : DbContext
    {
        public NetStandardContext(DbContextOptions<NetStandardContext> options) : base (options) { }

        public DbSet<Blog> Blogs { get; set; }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class NetStandardContextFactory : IDbContextFactory<NetStandardContext>
    {
        public NetStandardContext Create(DbContextFactoryOptions options)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NetStandardContext>();
            optionsBuilder.UseSqlite("Filename=./test.db");
            return new NetStandardContext(optionsBuilder.Options);
        }
    }
}