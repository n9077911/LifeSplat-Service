using System.Globalization;
using TaxCalcService.Models.DTO;
using TaxCalculator.ExternalInterface;

namespace TaxCalcService.Models
{
    public class TaxCalculatorDomainInterface : ITaxCalculatorDomainInterface
    {
        private readonly IIncomeTaxCalculator _taxCalculator;

        public TaxCalculatorDomainInterface(IIncomeTaxCalculator taxCalculator)
        {
            _taxCalculator = taxCalculator;
        }

        public TaxResultDto TaxFor(int payeSalary)
        {
            var taxResult = _taxCalculator.TaxFor(payeSalary);
            var calcIncomeTax = new TaxResultDto();
            calcIncomeTax.TaxResultItems.Add(new TaxResultItemDto {Amount = taxResult.IncomeTax.ToString(CultureInfo.InvariantCulture), Description = "Income Tax"});
            calcIncomeTax.TaxResultItems.Add(new TaxResultItemDto {Amount = taxResult.NationalInsurance.ToString(CultureInfo.InvariantCulture), Description = "National Ins."});
            calcIncomeTax.TaxResultItems.Add(new TaxResultItemDto {Amount = taxResult.TotalTax.ToString(CultureInfo.InvariantCulture), Description = "Total", IsTotal = true});

            return calcIncomeTax;
        }
    }
}