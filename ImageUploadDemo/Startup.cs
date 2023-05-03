using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ImageUploadDemo.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ImageUploadDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader()
                        .WithOrigins(
                            "http://localhost:3000", 
                            "https://ac-slide.azurewebsites.net", 
                            "https://ac-upload.azurewebsites.nüet",
                            "http://andersogceline.com",
                            "https://andersogceline.com"
                            )
                        .AllowCredentials();
                })
            );

            services.AddControllers();
            services.AddTransient<ConfigHelper>();
            services.AddTransient<NotifyService>();

            services.AddSignalR();
        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("CorsPolicy");
            
            app.UseHttpsRedirection();
            // app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ImageHub>("/imageHub");
            });
            
            // app.MapHub<ImageHub>("/imageHub");
            // app.UseMvc();
        }
    }
}
