using System;
using Calculator.ExternalInterface;

namespace Calculator.TaxSystem
{
    /// <summary>
    /// Calculates income tax and NI based a persons salary
    /// </summary>
    public class IncomeTaxCalculator : IIncomeTaxCalculator
    {
        private readonly ITaxSystem _taxSystem;

        public IncomeTaxCalculator()
        {
            _taxSystem = new England2020TaxSystem();
        }
        
        public IncomeTaxCalculator(ITaxSystem taxSystem)
        {
            _taxSystem = taxSystem;
        }

        public ITaxResult TaxFor(decimal payeSalary, decimal privatePension = 0, decimal statePension = 0, RentalIncomeForTax rentalIncome = null)
        {
            var result = new TaxResult();

            AddIncomeTax(payeSalary, privatePension, statePension, rentalIncome, result);
            AddNationalInsurance(payeSalary, result);

            return result;
        }

        private void AddNationalInsurance(decimal payeSalary, TaxResult result)
        {
            foreach (var band in _taxSystem.NationalInsuranceBands)
            {
                var taxable = Math.Min(payeSalary, band.UpperBound) - band.LowerBound;
                if (taxable > 0)
                    result.AddNationalInsurance(taxable * band.Rate, IncomeType.Salary);
            }
        }

        private void AddIncomeTax(decimal payeSalary, decimal privatePension, decimal statePension, RentalIncomeForTax rentalIncome, TaxResult result)
        {
            var taxBandTracker = TaxBandTracker.For(_taxSystem);
            
            taxBandTracker.UpdatePersonalAllowance(payeSalary, privatePension, statePension, rentalIncome?.GetIncomeToPayTaxOn() ?? 0);
            taxBandTracker = UpdateTaxResultWithIncome(result, payeSalary, IncomeType.Salary, taxBandTracker);
            taxBandTracker = UpdateTaxResultWithIncome(result, privatePension, IncomeType.PrivatePension, taxBandTracker);
            taxBandTracker = UpdateTaxResultWithIncome(result, statePension, IncomeType.StatePension, taxBandTracker);
            UpdateTaxResultWithIncome(result, rentalIncome?.GetIncomeToPayTaxOn() ?? 0, IncomeType.RentalIncome, taxBandTracker, rentalIncome?.GetNetIncome(),
                rentalIncome?.TaxDeductibleFinancingCosts() ?? 0);
        }

        private TaxBandTracker UpdateTaxResultWithIncome(TaxResult result, 
            decimal incomeToCalcTaxOn, 
            IncomeType incomeType, 
            TaxBandTracker taxBandTracker,
            decimal? incomeToRecord = null,
            decimal financingCosts = 0)
        {
            var incomeTracker = incomeToCalcTaxOn;
            
            result.AddIncomeFor(incomeToRecord ?? incomeToCalcTaxOn, incomeType);
            
            decimal totalTaxPaid = 0;
            if (incomeTracker > taxBandTracker.HigherRateUpperBound)
            {
                var extraHighBand = (incomeTracker - taxBandTracker.HigherRateUpperBound) * _taxSystem.AdditionalRate;
                totalTaxPaid += extraHighBand;
                result.AddIncomeTax(extraHighBand, incomeType);
                incomeTracker = taxBandTracker.HigherRateUpperBound;
            }

            if (incomeTracker > taxBandTracker.HigherBand)
            {
                var higherBandTax = (incomeTracker - taxBandTracker.HigherBand) * _taxSystem.HigherRate;
                totalTaxPaid = higherBandTax;
                result.AddIncomeTax(higherBandTax, incomeType);
                incomeTracker = taxBandTracker.HigherBand;
            }

            if (incomeTracker > taxBandTracker.PersonalAllowance)
            {
                var lowerBandTax = (incomeTracker - taxBandTracker.PersonalAllowance) * _taxSystem.BasicRate;
                totalTaxPaid += lowerBandTax;
                result.AddIncomeTax(lowerBandTax, incomeType);
            }

            if (IncomeType.RentalIncome == incomeType)
            {
                var taxCredit = financingCosts * _taxSystem.BasicRate;
                var usableTaxCredit = Math.Min(taxCredit, totalTaxPaid);
                result.AddRentalTaxCredit(usableTaxCredit);
            }

            return taxBandTracker.Subtract(incomeToCalcTaxOn);
        }
    }
}