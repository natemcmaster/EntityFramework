using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Reflection;
using System;

namespace PortableApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }

    public class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Console.WriteLine("Executor json version" + typeof(JsonConvert).GetTypeInfo().Assembly.GetName().Version);

            options.UseSqlite("Filename=./test.db");
        }
        
        public DbSet<Blog> Blogs { get; set; }
    }
    
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}