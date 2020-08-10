using System;
using System.Diagnostics;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    [DebuggerDisplay("{Date} : {Savings}")]
    public class Step
    {
        private readonly Step _previousStep;
        private readonly PersonStatus _personStatus;
        private readonly bool _calcdMinimum;
        private readonly DateTime? _givenRetirementDate;
        private decimal _monthly = 12m;

        public Step(Step previousStep, PersonStatus personStatus, bool calcdMinimum = false, DateTime? givenRetirementDate = null)
        {
            Date = previousStep.Date.AddMonths(1);
            Savings = previousStep.Savings;
            PrivatePensionAmount = previousStep.PrivatePensionAmount;
            _previousStep = previousStep;
            _personStatus = personStatus;
            _calcdMinimum = calcdMinimum;
            _givenRetirementDate = givenRetirementDate;
        }
        
        public Step()
        {
        }

        public DateTime Date { get; set; }
        public decimal StatePension { get; set; }
        public decimal PredictedStatePensionAnnual { get; set; }
        public decimal Growth { get; set; }
        public decimal AfterTaxSalary { get; set; }
        public decimal Savings { get; set; }
        public decimal PrivatePensionGrowth { get; set; }
        public decimal PrivatePensionAmount { get; set; }

        public void UpdateStatePensionAmount(
            IStatePensionAmountCalculator statePensionAmountCalculator, 
            PersonStatus personStatus,
            DateTime statePensionDate)
        {
            var statePensionAmount = Retired() ? _previousStep.PredictedStatePensionAnnual : statePensionAmountCalculator.Calculate(personStatus, Date);

            PredictedStatePensionAnnual = Convert.ToInt32(statePensionAmount);

            if (Date > statePensionDate)
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

        public void UpdateGrowth(decimal growthRate)
        {
            var growth = Math.Max(Savings * growthRate, 0m);
            Growth = growth;
            Savings += growth;
        }

        public void UpdatePrivatePension(decimal growthRate, DateTime privatePensionDate)
        {
            PrivatePensionGrowth = PrivatePensionAmount * growthRate;

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
    }
}