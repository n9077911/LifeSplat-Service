using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class StepsReport : IStepUpdater
    {
        private readonly PersonStatus _personStatus;
        private readonly StepType _stepType;
        private readonly IAssumptions _assumptions;
        private readonly DateTime _privatePensionDate;

        public StepsReport(PersonStatus personStatus, StepType stepType, DateTime now, IAssumptions assumptions, DateTime privatePensionDate)
        {
            _personStatus = personStatus;
            _stepType = stepType;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Steps.Add(new Step(now, personStatus.ExistingSavings, personStatus.ExistingPrivatePension, personStatus.ExistingPrivatePension * assumptions.MonthlyGrowthRate));
        }

        public List<Step> Steps { get; } = new List<Step>();

        public Step CurrentStep => Steps.Last();

        public void NewStep(bool calcdMinimum, DateTime? givenRetirementDate)
        {
            Steps.Add(_stepType == StepType.CalcMinimum 
                ? new Step(CurrentStep, _personStatus, calcdMinimum, _assumptions, _privatePensionDate) 
                : new Step(CurrentStep, _personStatus, false, _assumptions, _privatePensionDate, givenRetirementDate)); 
        }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate)
        {
            CurrentStep.UpdateStatePensionAmount(statePensionAmountCalculator, personStatePensionDate);
        }

        public void UpdateGrowth()
        {
            CurrentStep.UpdateGrowth();
        }

        public void UpdatePrivatePension(DateTime? givenRetirementDate)
        {
            CurrentStep.UpdatePrivatePension(givenRetirementDate);
        }

        public void UpdateSalary(decimal monthlyAfterTaxSalary)
        {
            CurrentStep.UpdateSalary(monthlyAfterTaxSalary);
        }

        public void UpdateSpending(decimal monthlySpending)
        {
            CurrentStep.UpdateSpending(monthlySpending);
        }

        public void SetSavings(decimal savings)
        {
            CurrentStep.SetSavings(savings);
        }
    }
}