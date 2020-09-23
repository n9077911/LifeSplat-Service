using System;
using System.Collections.Generic;
using System.Diagnostics;
using Calculator.Input;
using Calculator.TaxSystem;
using Calculator.ExternalInterface;

namespace Calculator
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

        public Step(Step previousStep, DateTime stepDate, Person person, bool calcdMinimum, IAssumptions assumptions, 
            DateTime privatePensionDate, decimal spending, DateTime? givenRetirementDate = null)
        {
            Date = stepDate;
            Savings = previousStep.Savings;
            EmergencyFund = previousStep.EmergencyFund;
            PrivatePensionAmount = previousStep.PrivatePensionAmount;
            _previousStep = previousStep;
            _person = person;
            _calcdMinimum = calcdMinimum;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Spending = spending;
            _givenRetirementDate = givenRetirementDate;
        }

        private Step(DateTime now, int existingSavings, int existingPrivatePension, EmergencyFundSpec emergencyFundSpec, decimal personMonthlySpending)
        {
            Date = now;

            var requiredCashSavings = emergencyFundSpec.RequiredSavings(personMonthlySpending);
            if (requiredCashSavings > existingSavings)
            {
                EmergencyFund = existingSavings;
            }
            else
            {
                Savings = existingSavings - requiredCashSavings;
                EmergencyFund = requiredCashSavings;
            }
            
            PrivatePensionAmount = existingPrivatePension;
            Spending = personMonthlySpending;
        }

        public static Step CreateInitialStep(DateTime now, int existingSavings, int existingPrivatePension, EmergencyFundSpec emergencyFundSpec, decimal personMonthlySpending)
        {
            return new Step(now, existingSavings, existingPrivatePension, emergencyFundSpec, personMonthlySpending);
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
        public decimal EmergencyFund { get; private set; }
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
            if (Savings > Spending)
                Savings -= Spending;
            else
            {
                EmergencyFund -= Spending - Savings;
                Savings = 0;
            }
        }

        public void SetSavings(decimal savings)
        {
            Savings = savings;
        }
        
        public void SetEmergencyFund(decimal savings)
        {
            EmergencyFund = savings;
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

            var newIncome = AfterTaxSalary + AfterTaxPrivatePensionIncome + AfterTaxStatePension;

            var requiredCash = _person.EmergencyFundSpec.RequiredSavings(Spending);
                var newlyRequiredCash = requiredCash - EmergencyFund;
                if (newlyRequiredCash <= 0) //we have more cash than needed so move it to investments
                    Savings += newlyRequiredCash * -1; 
                else if(newlyRequiredCash > 0 && newIncome >= newlyRequiredCash)//income more than fills the cash requirement then assign the relevant amount to cash and remainder to investments
                {
                    EmergencyFund = requiredCash;
                    newIncome -= newlyRequiredCash;
                }
                else if(newlyRequiredCash > 0 && newIncome < newlyRequiredCash) //income fails to fill the cash requirement then assign all income to cash.
                {
                    EmergencyFund += newIncome;
                    newIncome = 0;
                }

            Savings += newIncome;
            RebalanceInvestmentsAndCashSavings();
        }

        private void RebalanceInvestmentsAndCashSavings()
        {
            var requiredCash = _person.EmergencyFundSpec.RequiredSavings(Spending);
            if (EmergencyFund < requiredCash)
            {
                var newlyRequiredAmount = requiredCash - EmergencyFund;
                if (Savings > newlyRequiredAmount)
                {
                    EmergencyFund = requiredCash;
                    Savings -= newlyRequiredAmount;
                }
                else
                {
                    EmergencyFund += Savings;
                    Savings = 0;
                }
            }
        }
    }
}