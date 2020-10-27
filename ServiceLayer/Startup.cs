using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Calculator;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using ServiceLayer.Misc;
using ServiceLayer.Models;

namespace ServiceLayer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var cultureInfo = new CultureInfo("en-GB");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            
            services.AddApplicationInsightsTelemetry("5df2bfe9-d72c-4622-9fed-adbee51c0a02");
            
            services.AddControllers(options => { options.InputFormatters.Insert(0, new TextPlainInputFormatter()); });
            services.AddControllers().AddNewtonsoftJson();
            
            services.AddSingleton<ITaxSystem, TwentyTwentyTaxSystem>();
            services.AddSingleton<IIncomeTaxCalculator, IncomeTaxCalculator>();
            services.AddSingleton<ITaxCalculatorDomainInterface, TaxCalculatorDomainInterface>();
            services.AddSingleton<IDateProvider, DateProvider>();
            services.AddSingleton<IPensionAgeCalc, PensionAgeCalc>();
            services.AddSingleton<IStatePensionAmountCalculator, StatePensionAmountCalculator>();
            services.AddTransient<IAssumptions>(x => Assumptions.SafeWithdrawalNoInflationTake25Assumptions());
            services.AddSingleton<IRetirementCalculator, RetirementIncrementalApproachCalculator>();
            services.AddSingleton<IRetirementDomainInterface, RetirementDomainInterface>();
            
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // var telemetryClient = app.ApplicationServices.GetService<TelemetryClient>();
            
            // app.UseExceptionHandler(errorApp =>
            // {
            //     errorApp.Run(async context =>
            //     {
            //         var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            //         telemetryClient.TrackException(exceptionHandlerPathFeature.Error, new Dictionary<string, string> {{"Env", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}});
            //     });
            // });
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseCors(builder =>
            {
                if (env.IsDevelopment())
                    builder.WithOrigins("http://localhost:3000", "https://localhost:3000", 
                        "http://localhost:5000", "https://localhost:5000",
                        "http://localhost:8080", "https://localhost:8080",
                        "https://sctaxcalcservice.azurewebsites.net", "https://sctaxcalcservice.azurewebsites.net");
                
                if(env.IsProduction())
                    builder.WithOrigins("https://www.lifesplat.com", "https://lifesplat.com", 
                        "https://www.staging.lifesplat.com", "https://staging.lifesplat.com"); 
                    
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
        }
    }
}