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
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("MyPolicy",
            //        builder =>
            //        {
            //            builder
            //                .AllowAnyOrigin()
            //                .AllowAnyHeader()
            //                .AllowAnyMethod();
            //        });
            //});


            //services.AddSignalR(opt =>
            //        opt.KeepAliveInterval = TimeSpan.FromSeconds(10)
            //    //opt.KeepAliveInterval = TimeSpan.FromDays(1)
            //);


            services.AddCors(options => options.AddPolicy("CorsPolicy",
                builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader()
                        .WithOrigins(
                            "http://localhost:3000", 
                            "https://ac-slide.azurewebsites.net", 
                            "https://ac-upload.azurewebsites.net",
                            "http://andersogceline.com",
                            "https://andersogceline.com"
                            )
                        .AllowCredentials();
                })
            );

            services.AddMvc();
            services.AddTransient<ConfigHelper>();
            services.AddTransient<NotifyService>();
            //services.AddTransient<ImageHub>();
            //services.AddTransient<IHubContext, ImageHub>();

            services.AddSignalR();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseCors("CorsPolicy");
            //app.UseCors(b => b
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ImageHub>("/imageHub");
            });
            //app.UseMvc();
            
            
            
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("MVC didn't find anything!");
            //});

        }
    }
}
