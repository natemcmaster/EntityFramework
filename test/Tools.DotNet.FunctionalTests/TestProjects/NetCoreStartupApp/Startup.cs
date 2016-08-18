using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetStandardClassLibrary;

namespace NetCoreStartupApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDbContext<NetStandardContext>(o => o.UseSqlite("Filename=./lib.db", b => b.MigrationsAssembly("NetCoreStartupApp")));
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