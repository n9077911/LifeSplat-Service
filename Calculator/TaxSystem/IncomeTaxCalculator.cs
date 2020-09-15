using System;
using Calculator.ExternalInterface;

namespace Calculator.TaxSystem
{
    /// <summary>
    /// Calculates income tax and NI based a persons salary
    /// </summary>
    public class IncomeTaxCalculator : IIncomeTaxCalculator
    {
        public ITaxResult TaxFor(decimal payeSalary, decimal privatePension = 0, decimal statePension = 0)
        {
            var result = new TaxResult(payeSalary+privatePension+statePension);

            AddIncomeTax(payeSalary, privatePension, statePension, result);
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
                result.AddNationalInsurance(((lowerRatePay - lowerBound) * .12m), IncomeType.Salary);
            }

            if (payeSalary > midBound)
            {
                var midRatePay = Math.Min(upperBound, payeSalary);
                result.AddNationalInsurance(((midRatePay - midBound) * .02m), IncomeType.Salary);
            }
        }

        private static void AddIncomeTax(decimal payeSalary, decimal privatePension, decimal statePension, TaxResult result)
        {
            var taxBands = TaxBands.InitialEngland2020();

            taxBands.UpdatePersonalAllowance(payeSalary, privatePension, statePension);
            taxBands = UpdateTaxResultWithIncome(result, payeSalary, IncomeType.Salary, taxBands);
            taxBands = UpdateTaxResultWithIncome(result, privatePension, IncomeType.PrivatePension, taxBands);
            UpdateTaxResultWithIncome(result, statePension, IncomeType.StatePension, taxBands);
        }

        private static TaxBands UpdateTaxResultWithIncome(TaxResult result, decimal income, IncomeType incomeType, TaxBands taxBands)
        {
            decimal incomeCopy = income;
            result.AddIncomeFor(income, incomeType);
            if (income > taxBands.ExtraHighBand)
            {
                result.AddIncomeTax((income - taxBands.ExtraHighBand) * .45m, incomeType);
                income = taxBands.ExtraHighBand;
            }

            if (income > taxBands.HigherBand)
            {
                var higherBandTax = (income - taxBands.HigherBand) * .4m;
                result.AddIncomeTax(higherBandTax, incomeType);
                income = taxBands.HigherBand;
            }

            if (income > taxBands.PersonalAllowance)
            {
                var lowerBandTax = (income - taxBands.PersonalAllowance) * .2m;
                result.AddIncomeTax(lowerBandTax, incomeType);
            }

            return taxBands.Subtract(incomeCopy);
        }
    }
}