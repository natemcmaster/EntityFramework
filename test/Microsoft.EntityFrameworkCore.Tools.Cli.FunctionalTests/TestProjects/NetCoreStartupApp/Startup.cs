using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetStandardClassLibrary;
using Newtonsoft.Json;
using System.Reflection;

namespace StartupForNetStandardClassLibrary
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            System.Console.WriteLine("Inside json version"+ typeof(JsonConvert).GetTypeInfo().Assembly.GetName().Version);
            JsonConvert.SerializeObject(new object());
            services
                .AddDbContext<NetStandardContext>(o => o.UseSqlite("Filename=./lib.db"));
        }

        public void Configure(IApplicationBuilder app)
        {

        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}