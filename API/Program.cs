using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // NOTE: ручная миграция происходит снизу, 
            // то есть без всяких dotnet ef database update
            using var scope = host.Services.CreateScope();

            var services = scope.ServiceProvider;

            try
            {
                // services - это IServiceCollection из метода ConfigureServices в Startup
                // в этом же методе был добавлен DataContext в сервис
                var context = services.GetRequiredService<DataContext>();

                // метод создаст БД, если БД не существует, типа автоматизация
                // команды dotnet ef database update
                await context.Database.MigrateAsync();
                await Seed.SeedData(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during migration");
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
