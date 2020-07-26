using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class TaxResult : ITaxResult
    {
        private readonly int _payeSalary;

        public TaxResult(int payeSalary)
        {
            _payeSalary = payeSalary;
        }

        public decimal IncomeTax { get; private set; }
        public decimal NationalInsurance { get; private set; }
        public decimal Total { get; private set; }
        public decimal Remainder { get; private set; }

        public void AddIncomeTax(decimal incomeTax)
        {
            IncomeTax += incomeTax;
            Total += incomeTax;
            Remainder = _payeSalary - Total;
        }

        public void AddNationalInsurance(decimal nationalInsurance)
        {
            NationalInsurance += nationalInsurance;
            Total += nationalInsurance;
            Remainder = _payeSalary - Total;
        }
    }
}