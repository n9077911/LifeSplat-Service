using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    /// <summary>
    /// Builds up a report detailing the full list of Steps produced by the retirement calculator algorithm.
    /// Part of the output of the algorithm.
    /// </summary>
    public class StepsReport : IStepUpdater
    {
        private readonly PersonStatus _personStatus;
        private readonly StepType _stepType;
        private readonly IAssumptions _assumptions;
        private readonly DateTime _privatePensionDate;

        public StepsReport(PersonStatus personStatus, StepType stepType, DateTime now, IAssumptions assumptions, decimal monthlySpendingNow, DateTime privatePensionDate)
        {
            _personStatus = personStatus;
            _stepType = stepType;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Steps.Add(new Step(now, personStatus.ExistingSavings, personStatus.ExistingPrivatePension, monthlySpendingNow));
        }

        public List<Step> Steps { get; } = new List<Step>();

        public Step CurrentStep => Steps.Last();

        public void NewStep(bool calcdMinimum, ISpendingForDate spendingForDate, int numberOfPersons, DateTime? givenRetirementDate)
        {
            var newStepDate = CurrentStep.Date.AddMonths(1);
            
            var spending = spendingForDate.MonthlySpendingAt(newStepDate)/numberOfPersons;
                
            Steps.Add(_stepType == StepType.CalcMinimum 
            ? new Step(CurrentStep, newStepDate, _personStatus, calcdMinimum, _assumptions, _privatePensionDate, spending) 
            : new Step(CurrentStep, newStepDate, _personStatus, false, _assumptions, _privatePensionDate, spending, givenRetirementDate)); 
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

        public void UpdateSalary(decimal preTaxSalary)
        {
            CurrentStep.UpdateSalary(preTaxSalary);
        }

        public void UpdateSpending()
        {
            CurrentStep.UpdateSpending();
        }
        
        public void SetSavings(decimal savings)
        {
            CurrentStep.SetSavings(savings);
        }

        public void ProcessTaxableIncomeIntoSavings()
        {
            CurrentStep.PayTaxAndBankTheRemainder();
        }
    }

    public interface ISpendingForDate
    {
        decimal MonthlySpendingAt(DateTime date);
    }
}