using ImageUploadDemo.Hubs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageUploadDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // CreateWebHostBuilder(args).Build().Run();
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader()
                        .WithOrigins(
                            "https://ac-slideadmin.azurewebsites.net", 
                            "https://ac-up.azurewebsites.net"
                        )
                        .AllowCredentials();
                })
            );
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSignalR();
            
            builder.Services.AddTransient<ConfigHelper>();
            builder.Services.AddTransient<NotifyService>();
            
            var app = builder.Build();
            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();
            app.UseAuthorization();
            // app.UseRouting();
            app.MapControllers();
            
            //app.UseDefaultFiles();
            //app.UseStaticFiles();
            
            app.MapHub<ImageHub>("/imageHub");
            
            app.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}