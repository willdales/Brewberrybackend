using backend.Hardware;
using backend.Hubs;
using backend.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSignalR();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "backend", Version = "v1" });
            });
            services.AddTransient<TemperatureProbeManager>();
            services.AddSingleton<CountDownModule>();
            services.AddSingleton<TemperatureModule>();
            services.AddSingleton<PIDControlModule>();

            services.AddTransient<HardwareIOModule>();
            services.AddHostedService<TemperatureCollectionService>();
            services.AddSingleton<DisplayManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DisplayManager mgr)
        {
            app.UseDeveloperExceptionPage();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "backend v1"));
            }

            mgr.initDisplays();

            app.UseCors(builder =>

            {

                builder.AllowAnyMethod().AllowAnyHeader().AllowCredentials().AllowAnyOrigin().AllowCredentials()
                .WithOrigins("http://192.168.1.73:8080")
                
                .SetIsOriginAllowed((host) => true);


            });

         

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<CountDownHub>("/backend/countdownhub");
                endpoints.MapHub<TemperatureHub>("/backend/temperaturehub");
                endpoints.MapHub<PIDHub>("/backend/pidhub");
            });
        }
    }
}
