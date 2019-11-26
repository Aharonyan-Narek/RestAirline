﻿using System;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestAirline.Api.Filters;
using RestAirline.Api.HealthCheck;
using RestAirline.Api.Resources.Booking.Passenger.Add;
using RestAirline.CommandHandlers.Passenger;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace RestAirline.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        
        public IWebHostEnvironment Environment { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddControllers();

            services.AddMvc(options =>
                {
                    options.Filters.Add<UnhandledExceptionFilter>();
                    options.Filters.Add<ModelValidationFilter>();
                })
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<AddPassengerCommandValidator>());;

            services.AddCors();

            if (Environment.IsEnvironment("UnitTest"))
            {
                return ApplicationBootstrap.RegisterServicesForTesting(services);
            }

            return ApplicationBootstrap.RegisterServices(services, Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            var healthCheckResponseWriterProvider = new HealthCheckerResponseProvider(env);
            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = healthCheckResponseWriterProvider.Writer
            });

            app.UseHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = healthCheckResponseWriterProvider.Writer
            });
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}