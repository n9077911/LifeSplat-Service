using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaxCalcService.Misc;
using TaxCalcService.Models;
using TaxCalculator;
using TaxCalculator.ExternalInterface;

namespace TaxCalcService
{
    public class Startup
    {
        private const string EnabledOrigins = "enabledOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options => { options.InputFormatters.Insert(0, new TextPlainInputFormatter()); });
            services.AddCors(options =>
            {
                options.AddPolicy(name: EnabledOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000", "https://localhost:3000");
                    });
            });
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

            app.UseCors(EnabledOrigins);

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}