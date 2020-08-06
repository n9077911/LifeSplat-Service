using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class IncomeTaxCalculator : IIncomeTaxCalculator
    {
        public ITaxResult TaxFor(decimal payeSalary)
        {
            var result = new TaxResult(payeSalary);

            AddIncomeTax(payeSalary, result);
            AddNationalInsurance(payeSalary, result);

            return result;
        }

        private static void AddNationalInsurance(decimal payeSalary, TaxResult result)
        {
            int lowerBound = 9_500;
            int midBound = 50_000;
            int upperBound = Int32.MaxValue;

            if (payeSalary > lowerBound)
            {
                var lowerRatePay = Math.Min(midBound, payeSalary);
                result.AddNationalInsurance(((lowerRatePay - lowerBound) * .12m));
            }

            if (payeSalary > midBound)
            {
                var midRatePay = Math.Min(upperBound, payeSalary);
                result.AddNationalInsurance(((midRatePay - midBound) * .02m));
            }
        }

        private static void AddIncomeTax(decimal payeSalary, TaxResult result)
        {
            var personalAllowance = 12_509m;
            var lowerBand = 37_500;

            if (payeSalary > 100_000)
            {
                personalAllowance = Math.Max(0m, personalAllowance - (payeSalary - 100_000) / 2);
            }

            if (payeSalary <= personalAllowance)
            {
                return;
            }

            if (payeSalary > personalAllowance)
            {
                var cappedAmount = Math.Min(personalAllowance + lowerBand, payeSalary);
                result.AddIncomeTax((cappedAmount - personalAllowance) * .2m);
            }

            if (payeSalary > 50_000)
            {
                var cappedAmount = Math.Min(150_000, payeSalary);
                result.AddIncomeTax((cappedAmount - lowerBand - personalAllowance) * .4m);
            }

            if (payeSalary > 150_000)
            {
                var amount = payeSalary - 150_000;
                result.AddIncomeTax(amount * .45m);
            }
        }
    }
}