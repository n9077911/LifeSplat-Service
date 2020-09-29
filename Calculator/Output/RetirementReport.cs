using System;
using System.Collections.Generic;
using System.Linq;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.StatePensionCalculator;

namespace Calculator.Output
{
    internal class RetirementReport : IRetirementReport, ISpendingForDate
    {
        private readonly IAssumptions _assumptions;

        public RetirementReport(IPensionAgeCalc pensionAgeCalc, IIncomeTaxCalculator incomeTaxCalculator, Family family, DateTime now, DateTime? givenRetirementDate, IAssumptions assumptions)
        {
            _assumptions = assumptions;
            TimeToRetirement = new DateAmount(DateTime.MinValue, DateTime.MinValue); 
    
            var spendingStepInputs = family.SpendingStepInputs.OrderBy(input => input.Date).ToList();
            for (int i = 0; i < spendingStepInputs.Count; i++)
            {
                var endDate = i < spendingStepInputs.Count - 1 ? spendingStepInputs[i + 1].Date : family.PrimaryPerson.Dob.AddYears(100);
                SpendingSteps.Add(new SpendingStepReport(spendingStepInputs[i].Date, endDate.AddDays(-1), spendingStepInputs[i].NewAmount));
            }
            
            var monthlySpendingAt = MonthlySpendingAt(now)/family.Persons.Count;
            foreach (var person in family.Persons)
                Persons.Add(new PersonReport(pensionAgeCalc, incomeTaxCalculator, person, now, givenRetirementDate.HasValue, _assumptions, monthlySpendingAt));
        }

        public List<SpendingStepReport> SpendingSteps { get; } = new List<SpendingStepReport>();
        public DateAmount TimeToRetirement { get; set; }
        public DateTime BankruptDate { get; private set; } = DateTime.MaxValue;
        
        public DateTime? TargetRetirementDate { get; set; }
        public int? TargetRetirementAge { get; set; }

        public DateTime MinimumPossibleRetirementDate => PrimaryPerson.MinimumPossibleRetirementDate;
        public int MinimumPossibleRetirementAge => PrimaryPerson.MinimumPossibleRetirementAge;
        public int SavingsAtPrivatePensionAge => PrimaryPerson.SavingsCombinedAtPrivatePensionAge;
        public int SavingsAtStatePensionAge => PrimaryPerson.SavingsCombinedAtStatePensionAge;
        public int SavingsAtMinimumPossiblePensionAge => PrimaryPerson.SavingsAtMinimumPossiblePensionAge;
        public int PrivatePensionPotAtPrivatePensionAge => PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge;
        public int PrivatePensionPotAtStatePensionAge => PrimaryPerson.PrivatePensionPotCombinedAtStatePensionAge;
        public int SavingsAt100 { get; private set; }

        public IPersonReport PrimaryPerson => Persons.First();

        public List<IPersonReport> Persons { get; } = new List<IPersonReport>();

        public decimal MonthlySpendingAt(DateTime date)
        {
            var spendingStep = SpendingSteps.FirstOrDefault(step => date.Date >= step.StartDate.Date && date.Date <= step.EndDate.Date) ?? SpendingSteps.Last();
            return spendingStep.Spending / 12m;
        }

        public void UpdateMinimumPossibleInfo(DateTime minimumPossibleRetirementDate, int savingsAtMinimumPossiblePensionAge)
        {
            PrimaryPerson.SavingsAtMinimumPossiblePensionAge = savingsAtMinimumPossiblePensionAge;
            PrimaryPerson.UpdateMinimumPossibleRetirementDate(minimumPossibleRetirementDate);
            Persons.Last().SavingsAtMinimumPossiblePensionAge = savingsAtMinimumPossiblePensionAge;
            Persons.Last().UpdateMinimumPossibleRetirementDate(minimumPossibleRetirementDate);
        }

        public void ProcessResults(DateTime? givenRetirementDate, DateTime now)
        {
            UpdateResultsBasedOnSetDates();
            UpdatePersonResults();
            TimeToRetirement = new DateAmount(now, MinimumPossibleRetirementDate);
            TargetRetirementDate = givenRetirementDate;
            TargetRetirementAge = TargetRetirementDate.HasValue ? AgeCalc.Age(PrimaryPerson.Person.Dob, TargetRetirementDate.Value) : (int?) null;
        }

        private void UpdatePersonResults()
        {
            foreach (var personReport in Persons)
            {
                personReport.PrivatePensionSafeWithdrawal = Convert.ToInt32(personReport.PrivatePensionPotAtPrivatePensionAge * _assumptions.AnnualGrowthRate); 
                personReport.AnnualStatePension = Convert.ToInt32(personReport.StepReport.Steps.Last().PredictedStatePensionAnnual);
                personReport.NiContributingYears = personReport.StepReport.Steps.Last().NiContributingYears;
                personReport.StatePensionDate = personReport.StatePensionDate;
                personReport.PrivatePensionDate = personReport.PrivatePensionDate;
            
                personReport.BankruptAge = AgeCalc.Age(personReport.Person.Dob, BankruptDate);
                personReport.StatePensionAge = AgeCalc.Age(personReport.Person.Dob, personReport.StatePensionDate);
                personReport.PrivatePensionAge = AgeCalc.Age(personReport.Person.Dob, personReport.PrivatePensionDate);
            }
        }

        private void UpdateResultsBasedOnSetDates()
        {
            var bankrupt = false;
            var detailsSet = Persons.Select(p => new DetailsSet{Person = p, PrivatePensionSet = false, StatePensionSet=false}).ToList();
            
            for (int stepIndex = 0; stepIndex < PrimaryPerson.StepReport.Steps.Count; stepIndex++)
            {
                var stepDate = PrimaryPerson.StepReport.Steps[stepIndex].Date;
                if (Persons.Select(p => p.StepReport.Steps[stepIndex].Investments).Sum() + Persons.Select(p => p.StepReport.Steps[stepIndex].EmergencyFund).Sum() < 0 && !bankrupt)
                {
                    bankrupt = true;
                    BankruptDate = stepDate;
                }

                foreach (var details in detailsSet)
                {
                    details.PrivatePensionSet = UpdatePrivatePensionDetails(details.Person, stepDate, details.PrivatePensionSet, stepIndex);
                    details.StatePensionSet = UpdateStatePensionDetails(details.Person, stepDate, details.StatePensionSet, stepIndex);
                }
            }

            SavingsAt100 = Convert.ToInt32(TotalSavingsForIthStep(PrimaryPerson.StepReport.Steps.Count-1));
        }

        private bool UpdateStatePensionDetails(IPersonReport person, DateTime stepDate, bool statePensionSet, int stepIndex)
        {
            if (stepDate >= person.StatePensionDate && !statePensionSet)
            {
                statePensionSet = true;
                person.SavingsCombinedAtStatePensionAge = Convert.ToInt32(Math.Max(0, TotalSavingsForIthStep(stepIndex)));
                person.PrivatePensionPotCombinedAtStatePensionAge = Convert.ToInt32(PrivatePensionPotForIthStep(stepIndex));
            }

            return statePensionSet;
        }

        private bool UpdatePrivatePensionDetails(IPersonReport person, DateTime stepDate, bool privatePensionSet, int stepIndex)
        {
            if (stepDate >= person.PrivatePensionDate && !privatePensionSet)
            {
                privatePensionSet = true;
                person.SavingsCombinedAtPrivatePensionAge = Convert.ToInt32(Math.Max(0, TotalSavingsForIthStep(stepIndex)));
                person.PrivatePensionPotCombinedAtPrivatePensionAge = Convert.ToInt32(PrivatePensionPotForIthStep(stepIndex));
                person.PrivatePensionPotAtPrivatePensionAge = Convert.ToInt32(person.StepReport.Steps[stepIndex].PrivatePensionAmount);
            }

            return privatePensionSet;
        }

        private decimal TotalSavingsForIthStep(int i)
        {
            return Persons.Select(p => p.StepReport.Steps[i].Investments + p.StepReport.Steps[i].EmergencyFund).Sum();
        }
        
        private decimal PrivatePensionPotForIthStep(int i)
        {
            return Persons.Select(p => p.StepReport.Steps[i].PrivatePensionAmount).Sum();
        }
        
        public void BalanceSavings()
        {
            var calcMinSavings = Persons.Select(p => p.StepReport.CurrentStep.Investments).Sum();
            
            var calcMinEmergencyFund = Persons.Select(p => p.StepReport.CurrentStep.EmergencyFund).Sum();
            
            foreach (var person in Persons)
            {
                person.StepReport.SetSavings(calcMinSavings / Persons.Count);
                person.StepReport.SetEmergencyFund(calcMinEmergencyFund / Persons.Count);
            }
        }
    }

    public class DetailsSet
    {
        public IPersonReport Person { get; set; }
        public bool PrivatePensionSet { get; set; }
        public bool StatePensionSet { get; set; }
    }
    
    public interface ISpendingForDate
    {
        decimal MonthlySpendingAt(DateTime date);
    }
}