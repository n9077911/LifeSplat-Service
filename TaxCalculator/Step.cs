using System;
using System.Collections.Generic;
using System.Diagnostics;
using TaxCalculator.ExternalInterface;
using TaxCalculator.TaxSystem;

namespace TaxCalculator
{
    [DebuggerDisplay("{Date} : {Savings}")]
    public class Step : IStepUpdater
    {
        private readonly Step _previousStep;
        private readonly PersonStatus _personStatus;
        private readonly bool _calcdMinimum;
        private readonly IAssumptions _assumptions;
        private readonly DateTime _privatePensionDate;
        private readonly DateTime? _givenRetirementDate;
        private decimal _monthly = 12m;

        private decimal _preTaxSalary = 0;
        private decimal _annualPreTaxPrivatePensionIncome = 0;
        private decimal _monthlyPreTaxPrivatePensionIncome = 0;
        private decimal _preTaxStatePensionIncome = 0;

        public Step(Step previousStep, DateTime stepDate, PersonStatus personStatus, bool calcdMinimum, IAssumptions assumptions, DateTime privatePensionDate, decimal spending, DateTime? givenRetirementDate = null)
        {
            Date = stepDate;
            Savings = previousStep.Savings;
            PrivatePensionAmount = previousStep.PrivatePensionAmount;
            _previousStep = previousStep;
            _personStatus = personStatus;
            _calcdMinimum = calcdMinimum;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Spending = spending;
            _givenRetirementDate = givenRetirementDate;
        }
        
        public Step(DateTime now, int existingSavings, int existingPrivatePension, decimal personStatusMonthlySpending)
        {
            Date = now;
            Savings = existingSavings;
            PrivatePensionAmount = existingPrivatePension;
            Spending = personStatusMonthlySpending;
        }

        public DateTime Date { get; private set; }
        public decimal PredictedStatePensionAnnual { get; private set; }
        public int NiContributingYears { get; private set; }
        public decimal Growth { get; private set; }
        public decimal AfterTaxSalary { get; private set; }
        public decimal AfterTaxStatePension { get; private set; }
        public decimal AfterTaxPrivatePensionIncome { get; private set; }
        
        public decimal Spending { get; private set; }
        public decimal Savings { get; private set; }
        public decimal PrivatePensionAmount { get; private set; }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate)
        {
            if (QuitWork())
            {
                PredictedStatePensionAnnual = _previousStep.PredictedStatePensionAnnual;
                NiContributingYears = _previousStep.NiContributingYears;
            }
            else
            {
                var predictedStatePensionAnnual = statePensionAmountCalculator.Calculate(_personStatus, Date);
                NiContributingYears = predictedStatePensionAnnual.Item1;
                PredictedStatePensionAnnual = Convert.ToInt32(predictedStatePensionAnnual.Item2);
                // PredictedStatePensionAnnual = predictedStatePensionAnnual.Item2;
            }

            if (Date > personStatePensionDate)
            {
                _preTaxStatePensionIncome = PredictedStatePensionAnnual/_monthly;
            }
        }

        private bool QuitWork() => _givenRetirementDate.HasValue ? Date > _givenRetirementDate : _calcdMinimum;

        public void UpdateGrowth()
        {
            var growth = Math.Max(Savings * _assumptions.MonthlyGrowthRate, 0m);
            Growth = growth;
            Savings += growth;
        }

        public void UpdatePrivatePension(DateTime? givenRetirementDate)
        {
            var privatePensionGrowth = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;

            if (Date >= _privatePensionDate && QuitWork())
            {
                //must be annualised (using the monthly figure and multiplying by 12 won't work as 12*monthlyRate != annualRate - because the monthly rate assumes compounding!
                _annualPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.AnnualGrowthRate;
                _monthlyPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;
            }
            else
                PrivatePensionAmount += privatePensionGrowth;
            
            if (!QuitWork())
                PrivatePensionAmount += (_personStatus.Salary / _monthly) * (_personStatus.EmployeeContribution + _personStatus.EmployerContribution);
        }

        public void UpdateSalary(decimal preTaxSalary)
        {
            if (!QuitWork())
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
            AfterTaxSalary = afterTax.RemainderFor(IncomeType.Salary)/12;
            AfterTaxPrivatePensionIncome = _monthlyPreTaxPrivatePensionIncome - (afterTax.TotalTaxFor(IncomeType.PrivatePension)/12);
            AfterTaxStatePension = afterTax.RemainderFor(IncomeType.StatePension)/12;
            Savings += AfterTaxSalary + AfterTaxPrivatePensionIncome + AfterTaxStatePension;
        }
    }
}