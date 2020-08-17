using System;
using System.Diagnostics;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    [DebuggerDisplay("{Date} : {Savings}")]
    public class Step : IStepUpdater
    {
        private readonly Step _previousStep;
        private readonly PersonStatus _personStatus;
        private readonly bool _calcdMinimum;
        private readonly IAssumptions _assumptions;
        private readonly DateTime? _givenRetirementDate;
        private decimal _monthly = 12m;

        public Step(Step previousStep, PersonStatus personStatus, bool calcdMinimum, IAssumptions assumptions, DateTime? givenRetirementDate = null)
        {
            Date = previousStep.Date.AddMonths(1);
            Savings = previousStep.Savings;
            PrivatePensionAmount = previousStep.PrivatePensionAmount;
            _previousStep = previousStep;
            _personStatus = personStatus;
            _calcdMinimum = calcdMinimum;
            _assumptions = assumptions;
            _givenRetirementDate = givenRetirementDate;
        }
        
        public Step(DateTime now, int existingSavings, int existingPrivatePension, decimal existingPensionGrowth)
        {
            Date = now;
            Savings = existingSavings;
            PrivatePensionAmount = existingPrivatePension;
            PrivatePensionGrowth = existingPensionGrowth; //todo: why is growth added here? this is for the initial step
        }

        public DateTime Date { get; private set; }
        public decimal StatePension { get; private set; }
        public decimal PredictedStatePensionAnnual { get; private set; }
        public decimal Growth { get; private set; }
        public decimal AfterTaxSalary { get; private set; }
        public decimal Savings { get; private set; }
        public decimal PrivatePensionGrowth { get; private set; }
        public decimal PrivatePensionAmount { get; private set; }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate)
        {
            var statePensionAmount = Retired() ? _previousStep.PredictedStatePensionAnnual : statePensionAmountCalculator.Calculate(_personStatus, Date);

            PredictedStatePensionAnnual = Convert.ToInt32(statePensionAmount);

            if (Date > personStatePensionDate)
            {
                Savings += PredictedStatePensionAnnual / _monthly;
                StatePension = PredictedStatePensionAnnual / _monthly;
            }
        }

        private bool Retired()
        {
            if(_givenRetirementDate.HasValue)
                return Date > _givenRetirementDate;
            return _calcdMinimum;
        }

        public void UpdateGrowth()
        {
            var growth = Math.Max(Savings * _assumptions.MonthlyGrowthRate, 0m);
            Growth = growth;
            Savings += growth;
        }

        public void UpdatePrivatePension(DateTime privatePensionDate)
        {
            PrivatePensionGrowth = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;

            //TODO: needs to consider if you work past private pension age.
            if (Date >= privatePensionDate) //and retired
                Savings += PrivatePensionGrowth;
            else
                PrivatePensionAmount += PrivatePensionGrowth;
            
            if (!Retired())
                PrivatePensionAmount += (_personStatus.Salary / _monthly) * (_personStatus.EmployeeContribution + _personStatus.EmployerContribution);
        }

        public void UpdateSalary(decimal monthlyAfterTaxSalary)
        {
            if (!Retired())
            {
                Savings += monthlyAfterTaxSalary;
                AfterTaxSalary = monthlyAfterTaxSalary;
            }
        }

        public void UpdateSpending(decimal monthlySpending)
        {
            Savings -= monthlySpending;
        }

        public void SetSavings(decimal savings)
        {
            Savings = savings;
        }
    }
}