using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaxCalcService.Misc;
using TaxCalcService.Models;
using TaxCalculator;
using TaxCalculator.ExternalInterface;
using TaxCalculator.TaxSystem;

namespace TaxCalcService
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
            
            
            services.AddControllers(options => { options.InputFormatters.Insert(0, new TextPlainInputFormatter()); });
            services.AddControllers().AddNewtonsoftJson();
            
            services.AddSingleton<ITaxSystem, TwentyTwentyTaxSystem>();
            services.AddSingleton<IIncomeTaxCalculator, IncomeTaxCalculator>();
            services.AddSingleton<ITaxCalculatorDomainInterface, TaxCalculatorDomainInterface>();
            services.AddSingleton<IDateProvider, DateProvider>();
            services.AddSingleton<IPensionAgeCalc, PensionAgeCalc>();
            services.AddSingleton<IStatePensionAmountCalculator, StatePensionAmountCalculator>();
            services.AddSingleton<IAssumptions, SafeWithdrawalNoInflationAssumptions>();
            services.AddSingleton<IRetirementCalculator, RetirementIncrementalApproachCalculator>();
            services.AddSingleton<IRetirementDomainInterface, RetirementDomainInterface>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseCors(builder =>
            {
                if (env.IsDevelopment())
                    builder.WithOrigins("http://localhost:3000", "https://localhost:3000",
                        "https://sctaxcalcservice.azurewebsites.net", "https://sctaxcalcservice.azurewebsites.net");
                
                if(env.IsProduction())
                    builder.WithOrigins("https://www.lifesplat.com", "https://lifesplat.com"); 
                
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
            });
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}