using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class TaxResult : ITaxResult
    {
        public decimal IncomeTax { get; private set; }
        public decimal NationalInsurance { get; private set; }
        public decimal Total { get; private set; }

        public void AddIncomeTax(decimal incomeTax)
        {
            IncomeTax += incomeTax;
            Total += incomeTax;
        }

        public void AddNationalInsurance(decimal nationalInsurance)
        {
            NationalInsurance += nationalInsurance;
            Total += nationalInsurance;
        }
    }
}