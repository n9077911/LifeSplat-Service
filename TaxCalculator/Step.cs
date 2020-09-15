using System;
using System.Collections.Generic;
using System.Diagnostics;
using TaxCalculator.ExternalInterface;
using TaxCalculator.Input;
using TaxCalculator.TaxSystem;

namespace TaxCalculator
{
    /// <summary>
    /// Represents 1 month of a persons future life.
    /// Used by RetirementIncrementalApproachCalculator to keep track of a persons finances as the calculator iterates through the persons future. 
    /// </summary>
    [DebuggerDisplay("{Date} : {Savings}")]
    public class Step
    {
        private readonly Step _previousStep;
        private readonly Person _person;
        private readonly bool _calcdMinimum;
        private readonly IAssumptions _assumptions;
        private readonly DateTime _privatePensionDate;
        private readonly DateTime? _givenRetirementDate;
        private decimal _monthly = 12m;

        private decimal _preTaxSalary = 0;
        private decimal _annualPreTaxPrivatePensionIncome = 0;
        private decimal _monthlyPreTaxPrivatePensionIncome = 0;
        private decimal _preTaxStatePensionIncome = 0;

        public Step(Step previousStep, DateTime stepDate, Person person, bool calcdMinimum, IAssumptions assumptions, DateTime privatePensionDate, decimal spending, DateTime? givenRetirementDate = null)
        {
            Date = stepDate;
            Savings = previousStep.Savings;
            PrivatePensionAmount = previousStep.PrivatePensionAmount;
            _previousStep = previousStep;
            _person = person;
            _calcdMinimum = calcdMinimum;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Spending = spending;
            _givenRetirementDate = givenRetirementDate;
        }

        private Step(DateTime now, int existingSavings, int existingPrivatePension, decimal personStatusMonthlySpending)
        {
            Date = now;
            Savings = existingSavings;
            PrivatePensionAmount = existingPrivatePension;
            Spending = personStatusMonthlySpending;
        }

        public static Step CreateInitialStep(DateTime now, int existingSavings, int existingPrivatePension, decimal personStatusMonthlySpending)
        {
            return new Step(now, existingSavings, existingPrivatePension, personStatusMonthlySpending);
        }

        public DateTime Date { get; private set; }
        public decimal PredictedStatePensionAnnual { get; private set; }
        public int NiContributingYears { get; private set; }
        public decimal Growth { get; private set; }
        public decimal AfterTaxSalary { get; private set; }
        public decimal AfterTaxStatePension { get; private set; }
        public decimal AfterTaxPrivatePensionIncome { get; private set; }
        
        public decimal Spending { get; }
        public decimal Savings { get; private set; }
        public decimal PrivatePensionAmount { get; private set; }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate)
        {
            if (PersonHasQuitWork())
            {
                PredictedStatePensionAnnual = _previousStep.PredictedStatePensionAnnual;
                NiContributingYears = _previousStep.NiContributingYears;
            }
            else
            {
                var predictedStatePensionAnnual = statePensionAmountCalculator.Calculate(_person, Date);
                NiContributingYears = predictedStatePensionAnnual.ContributingYears;
                PredictedStatePensionAnnual = Convert.ToInt32(predictedStatePensionAnnual.Amount);
            }

            if (Date > personStatePensionDate)
            {
                _preTaxStatePensionIncome = PredictedStatePensionAnnual/_monthly;
            }
        }

        private bool PersonHasQuitWork() => _givenRetirementDate.HasValue ? Date > _givenRetirementDate : _calcdMinimum;

        public void UpdateGrowth()
        {
            var growth = Math.Max(Savings * _assumptions.MonthlyGrowthRate, 0m);
            Growth = growth;
            Savings += growth;
        }

        public void UpdatePrivatePension()
        {
            var privatePensionGrowth = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;

            if (Date >= _privatePensionDate && PersonHasQuitWork())
            {
                //must be annualised (using the monthly figure and multiplying by 12 won't work as 12*monthlyRate != annualRate - because the monthly rate assumes compounding!
                _annualPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.AnnualGrowthRate;
                _monthlyPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;
            }
            else
                PrivatePensionAmount += privatePensionGrowth;
            
            if (!PersonHasQuitWork())
                PrivatePensionAmount += (_person.Salary / _monthly) * (_person.EmployeeContribution + _person.EmployerContribution);
        }

        public void UpdateSalary(decimal preTaxSalary)
        {
            if (!PersonHasQuitWork())
            {
                _preTaxSalary = preTaxSalary;
            }
        }

        public void UpdateSpending()
        {
            Savings -= Spending;
        }

        public void SetSavings(decimal savings)
        {
            Savings = savings;
        }

        public void PayTaxAndBankTheRemainder()
        {
            //Simplification - calculate tax for the whole year then divide it for the month
            //this will not be accurate on years where someone quits work or starts receiving a pension.
            //In that case the fact they work\receive pension for a partial year would mean their real tax bill is less that calculated here
            var incomeTaxCalculator = new IncomeTaxCalculator();
            var afterTax = incomeTaxCalculator.TaxFor(_preTaxSalary*12, _annualPreTaxPrivatePensionIncome, _preTaxStatePensionIncome*12);
            AfterTaxSalary = afterTax.AfterTaxIncomeFor(IncomeType.Salary)/12;
            AfterTaxPrivatePensionIncome = _monthlyPreTaxPrivatePensionIncome - (afterTax.TotalTaxFor(IncomeType.PrivatePension)/12);
            AfterTaxStatePension = afterTax.AfterTaxIncomeFor(IncomeType.StatePension)/12;
            Savings += AfterTaxSalary + AfterTaxPrivatePensionIncome + AfterTaxStatePension;
        }
    }
}