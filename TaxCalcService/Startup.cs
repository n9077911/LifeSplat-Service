using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });


            services.AddControllers(options => { options.InputFormatters.Insert(0, new TextPlainInputFormatter()); });
            services.AddCors(options =>
            {
                // options.AddPolicy(name: EnabledOrigins,
                //     builder =>
                //     {
                //         builder.WithOrigins("http://localhost:3000", "https://localhost:3000").AllowAnyOrigin().AllowAnyMethod();
                //     });
                //
                options.AddPolicy(name: EnabledOrigins,
                    builder => { builder.AllowAnyOrigin().AllowAnyMethod(); });
            });

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

    public class TextPlainInputFormatter : TextInputFormatter
    {
        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add("text/plain");
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedEncodings.Add(UTF16EncodingLittleEndian);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context,
            Encoding encoding)
        {
            string data = null;
            using (var streamReader = context.ReaderFactory(context.HttpContext.Request.Body, encoding))
            {
                data = await streamReader.ReadToEndAsync();
            }

            return InputFormatterResult.Success(data);
        }
    }
}