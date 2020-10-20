using System;
using System.Collections.Generic;
using System.Linq;
using Calculator.Input;
using Calculator.StatePensionCalculator;

namespace Calculator.Output
{
    /// <summary>
    /// A report detailing the full list of Steps produced by the retirement calculator algorithm.
    /// Part of the output of the algorithm.
    /// </summary>
    public class StepsReport
    {
        private readonly Person _person;
        private readonly StepType _stepType;
        private readonly IAssumptions _assumptions;
        private readonly DateTime _privatePensionDate;

        public StepsReport(Person person, StepType stepType, DateTime now, IAssumptions assumptions, decimal monthlySpendingNow, DateTime privatePensionDate)
        {
            _person = person;
            _stepType = stepType;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Steps.Add(Step.CreateInitialStep(now, person.ExistingSavings, person.ExistingPrivatePension, person.EmergencyFundSpec, monthlySpendingNow));
        }

        private StepsReport(Person person, StepType stepType, IAssumptions assumptions, in DateTime privatePensionDate, Step step)
        {
            _person = person;
            _stepType = stepType;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Steps.Add(step);
        }

        public List<Step> Steps { get; } = new List<Step>();

        public Step CurrentStep => Steps.Last();

        public void NewStep(bool calcdMinimum, ISpendingForDate spendingForDate, int numberOfPersons, DateTime? givenRetirementDate)
        {
            var newStepDate = CurrentStep.StepDate.AddMonths(1);
            
            var spending = spendingForDate.MonthlySpendingAt(newStepDate)/numberOfPersons;
                
            Steps.Add(_stepType == StepType.CalcMinimum 
            ? new Step(CurrentStep, newStepDate, _person, calcdMinimum, _assumptions, _privatePensionDate, spending) 
            : new Step(CurrentStep, newStepDate, _person, false, _assumptions, _privatePensionDate, spending, givenRetirementDate)); 
        }

        public StepsReport CopyFromCurrent()
        {
            return new StepsReport(_person, _stepType, _assumptions, _privatePensionDate, CurrentStep.CopyForCalcMinimumMode());
        }

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate)
        {
            CurrentStep.UpdateStatePensionAmount(statePensionAmountCalculator, personStatePensionDate);
        }

        public void UpdateGrowth()
        {
            CurrentStep.UpdateGrowth();
        }

        public void UpdatePrivatePension()
        {
            CurrentStep.UpdatePrivatePension();
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
        
        public void SetEmergencyFund(decimal savings)
        {
            CurrentStep.SetEmergencyFund(savings);
        }

        public void ProcessTaxableIncomeIntoSavings()
        {
            CurrentStep.PayTaxAndBankTheRemainder();
        }

        public Take25Result CalcTake25()
        {
            return CurrentStep.CalcTake25();
        }

        public void Take25(Take25Result take25Result)
        {
            CurrentStep.Take25(take25Result);
        }

        public void UpdateChildBenefit(IPersonReport partner)
        {
            CurrentStep.CalculateChildBenefit(partner);
        }

        public decimal CalcLtaCharge()
        {
            return CurrentStep.CalcLtaCharge();
        }

        public void PayLtaCharge(in decimal ltaCharge)
        {
            CurrentStep.PayLtaCharge(ltaCharge);
        }
    }
}