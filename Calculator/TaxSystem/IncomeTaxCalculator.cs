using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Calculator.ExternalInterface;

namespace Calculator.TaxSystem
{
    /// <summary>
    /// Calculates income tax and NI based a persons salary
    /// </summary>
    public class IncomeTaxCalculator : IIncomeTaxCalculator
    {
        public ITaxResult TaxFor(decimal payeSalary, decimal privatePension = 0, decimal statePension = 0, RentalIncomeForTax rentalIncome = null)
        {
            var result = new TaxResult();

            AddIncomeTax(payeSalary, privatePension, statePension, rentalIncome, result);
            AddNationalInsurance(payeSalary, result);

            return result;
        }

        private static void AddNationalInsurance(decimal payeSalary, TaxResult result)
        {
            var taxBands = new TaxBands();

            foreach (var band in taxBands.NationalInsuranceBands)
            {
                var taxable = Math.Min(payeSalary, band.UpperBound) - band.LowerBound;
                if (taxable > 0)
                    result.AddNationalInsurance(taxable * band.Rate, IncomeType.Salary);
            }
        }

        private static void AddIncomeTax(decimal payeSalary, decimal privatePension, decimal statePension, RentalIncomeForTax rentalIncome, TaxResult result)
        {
            var taxBands = TaxBandTracker.InitialEngland2020();
            
            taxBands.UpdatePersonalAllowance(payeSalary, privatePension, statePension, rentalIncome?.GetIncomeToPayTaxOn() ?? 0);
            taxBands = UpdateTaxResultWithIncome(result, payeSalary, IncomeType.Salary, taxBands);
            taxBands = UpdateTaxResultWithIncome(result, privatePension, IncomeType.PrivatePension, taxBands);
            taxBands = UpdateTaxResultWithIncome(result, statePension, IncomeType.StatePension, taxBands);
            UpdateTaxResultWithIncome(result, rentalIncome?.GetIncomeToPayTaxOn() ?? 0, IncomeType.RentalIncome, taxBands, rentalIncome?.GetNetIncome(),
                rentalIncome?.TaxDeductibleFinancingCosts() ?? 0);
        }

        private static TaxBandTracker UpdateTaxResultWithIncome(TaxResult result, decimal incomeToCalcTaxOn, IncomeType incomeType, TaxBandTracker taxBandTracker, decimal? incomeToRecord = null, decimal financingCosts = 0)
        {
            decimal incomeTracker = incomeToCalcTaxOn;
            
            if(incomeToRecord.HasValue && incomeToRecord != incomeToCalcTaxOn)
                result.AddIncomeFor(incomeToRecord.Value, incomeType);
            else
                result.AddIncomeFor(incomeTracker, incomeType);
            
            decimal totalTaxPaid = 0;
            if (incomeTracker > taxBandTracker.ExtraHighBand)
            {
                var extraHighBand = (incomeTracker - taxBandTracker.ExtraHighBand) * .45m;
                totalTaxPaid += extraHighBand;
                result.AddIncomeTax(extraHighBand, incomeType);
                incomeTracker = taxBandTracker.ExtraHighBand;
            }

            if (incomeTracker > taxBandTracker.HigherBand)
            {
                var higherBandTax = (incomeTracker - taxBandTracker.HigherBand) * .4m;
                totalTaxPaid = higherBandTax;
                result.AddIncomeTax(higherBandTax, incomeType);
                incomeTracker = taxBandTracker.HigherBand;
            }

            if (incomeTracker > taxBandTracker.PersonalAllowance)
            {
                var lowerBandTax = (incomeTracker - taxBandTracker.PersonalAllowance) * .2m;
                totalTaxPaid += lowerBandTax;
                result.AddIncomeTax(lowerBandTax, incomeType);
            }

            if (IncomeType.RentalIncome == incomeType)
            {
                var taxCredit = financingCosts * .2m;
                var usableTaxCredit = Math.Min(taxCredit, totalTaxPaid);
                result.AddRentalTaxCredit(usableTaxCredit);
            }

            return taxBandTracker.Subtract(incomeToCalcTaxOn);
        }
    }

    internal class TaxBands
    {
        public IList<TaxBand> NationalInsuranceBands { get; } = new[] {new TaxBand(Int32.MaxValue, 50_000, .02m),  new TaxBand(50_000, 9_500, .12m) };
    }

    internal class TaxBand
    {
        public int UpperBound { get; }
        public int LowerBound { get; }
        public decimal Rate { get; }

        public TaxBand(int upperBound, int lowerBound, decimal rate)
        {
            UpperBound = upperBound;
            LowerBound = lowerBound;
            Rate = rate;
        }
    }
}