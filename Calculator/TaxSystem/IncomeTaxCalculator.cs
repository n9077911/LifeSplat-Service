﻿using System;
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

        private static void AddIncomeTax(decimal payeSalary, decimal privatePension, decimal statePension, RentalIncomeForTax rentalIncome, TaxResult result)
        {
            var taxBands = TaxBands.InitialEngland2020();
            
            taxBands.UpdatePersonalAllowance(payeSalary, privatePension, statePension, rentalIncome?.GetIncomeToPayTaxOn() ?? 0);
            taxBands = UpdateTaxResultWithIncome(result, payeSalary, IncomeType.Salary, taxBands);
            taxBands = UpdateTaxResultWithIncome(result, privatePension, IncomeType.PrivatePension, taxBands);
            taxBands = UpdateTaxResultWithIncome(result, statePension, IncomeType.StatePension, taxBands);
            UpdateTaxResultWithIncome(result, rentalIncome?.GetIncomeToPayTaxOn() ?? 0, IncomeType.RentalIncome, taxBands, rentalIncome?.GetNetIncome(),
                rentalIncome?.TaxDeductibleFinancingCosts() ?? 0);
        }

        private static TaxBands UpdateTaxResultWithIncome(TaxResult result, decimal incomeToCalcTaxOn, IncomeType incomeType, TaxBands taxBands, decimal? incomeToRecord = null, decimal financingCosts = 0)
        {
            decimal incomeTracker = incomeToCalcTaxOn;
            
            if(incomeToRecord.HasValue && incomeToRecord != incomeToCalcTaxOn)
                result.AddIncomeFor(incomeToRecord.Value, incomeType);
            else
                result.AddIncomeFor(incomeTracker, incomeType);
            
            decimal totalTaxPaid = 0;
            if (incomeTracker > taxBands.ExtraHighBand)
            {
                var extraHighBand = (incomeTracker - taxBands.ExtraHighBand) * .45m;
                totalTaxPaid += extraHighBand;
                result.AddIncomeTax(extraHighBand, incomeType);
                incomeTracker = taxBands.ExtraHighBand;
            }

            if (incomeTracker > taxBands.HigherBand)
            {
                var higherBandTax = (incomeTracker - taxBands.HigherBand) * .4m;
                totalTaxPaid = higherBandTax;
                result.AddIncomeTax(higherBandTax, incomeType);
                incomeTracker = taxBands.HigherBand;
            }

            if (incomeTracker > taxBands.PersonalAllowance)
            {
                var lowerBandTax = (incomeTracker - taxBands.PersonalAllowance) * .2m;
                totalTaxPaid += lowerBandTax;
                result.AddIncomeTax(lowerBandTax, incomeType);
            }

            if (IncomeType.RentalIncome == incomeType)
            {
                var taxCredit = financingCosts * .2m;
                var usableTaxCredit = Math.Min(taxCredit, totalTaxPaid);
                result.AddRentalTaxCredit(usableTaxCredit);
            }

            return taxBands.Subtract(incomeToCalcTaxOn);
        }
    }
}