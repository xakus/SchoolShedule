using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GenerationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://*:5004", "https://*:5005");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
