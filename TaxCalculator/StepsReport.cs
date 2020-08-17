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
        private readonly List<Step> _steps = new List<Step>();

        public StepsReport(PersonStatus personStatus, StepType stepType, DateTime now, IAssumptions assumptions)
        {
            _personStatus = personStatus;
            _stepType = stepType;
            _assumptions = assumptions;
            _steps.Add(new Step(now, personStatus.ExistingSavings, personStatus.ExistingPrivatePension, personStatus.ExistingPrivatePension * assumptions.MonthlyGrowthRate));
        }

        public List<Step> Steps => _steps;
        
        public Step CurrentStep => _steps.Last();

        public void NewStep(bool calcdMinimum, DateTime? givenRetirementDate)
        {
            _steps.Add(_stepType == StepType.CalcMinimum 
                ? new Step(CurrentStep, _personStatus, calcdMinimum, _assumptions) 
                : new Step(CurrentStep, _personStatus, false, _assumptions, givenRetirementDate)); 
        }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate)
        {
            CurrentStep.UpdateStatePensionAmount(statePensionAmountCalculator, personStatePensionDate);
        }

        public void UpdateGrowth()
        {
            CurrentStep.UpdateGrowth();
        }

        public void UpdatePrivatePension(DateTime privatePensionDate)
        {
            CurrentStep.UpdatePrivatePension(privatePensionDate);
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