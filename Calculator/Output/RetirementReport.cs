using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;
using TaxCalculator.Input;
using TaxCalculator.StatePensionCalculator;

namespace TaxCalculator.Output
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

        public void UpdatePersonResults()
        {
            foreach (var personReport in Persons)
            {
                personReport.PrivatePensionSafeWithdrawal = Convert.ToInt32(personReport.PrivatePensionPotAtPrivatePensionAge * _assumptions.AnnualGrowthRate); 
                personReport.AnnualStatePension = Convert.ToInt32(personReport.PrimarySteps.Steps.Last().PredictedStatePensionAnnual);
                personReport.NiContributingYears = personReport.PrimarySteps.Steps.Last().NiContributingYears;
                personReport.StatePensionDate = personReport.StatePensionDate;
                personReport.PrivatePensionDate = personReport.PrivatePensionDate;
            
                personReport.BankruptAge = AgeCalc.Age(personReport.Person.Dob, BankruptDate);
                personReport.StatePensionAge = AgeCalc.Age(personReport.Person.Dob, personReport.StatePensionDate);
                personReport.PrivatePensionAge = AgeCalc.Age(personReport.Person.Dob, personReport.PrivatePensionDate);
            }
        }
        
        public void UpdateResultsBasedOnSetDates()
        {
            var bankrupt = false;
            var detailsSet = Persons.Select(p => new DetailsSet{Person = p, PrivatePensionSet = false, StatePensionSet=false}).ToList();
            
            for (int stepIndex = 0; stepIndex < PrimaryPerson.CalcMinimumSteps.Steps.Count; stepIndex++)
            {
                var stepDate = PrimaryPerson.CalcMinimumSteps.Steps[stepIndex].Date;
                if (Persons.Select(p => p.PrimarySteps.Steps[stepIndex].Savings).Sum() < 0 && !bankrupt)
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

            SavingsAt100 = Convert.ToInt32(SavingsForIthStep(PrimaryPerson.CalcMinimumSteps.Steps.Count-1));
        }

        private bool UpdateStatePensionDetails(IPersonReport person, DateTime stepDate, bool statePensionSet, int stepIndex)
        {
            if (stepDate >= person.StatePensionDate && !statePensionSet)
            {
                statePensionSet = true;
                person.SavingsCombinedAtStatePensionAge = Convert.ToInt32(Math.Max(0, SavingsForIthStep(stepIndex)));
                person.PrivatePensionPotCombinedAtStatePensionAge = Convert.ToInt32(PrivatePensionPotForIthStep(stepIndex));
            }

            return statePensionSet;
        }

        private bool UpdatePrivatePensionDetails(IPersonReport person, DateTime stepDate, bool privatePensionSet, int stepIndex)
        {
            if (stepDate >= person.PrivatePensionDate && !privatePensionSet)
            {
                privatePensionSet = true;
                person.SavingsCombinedAtPrivatePensionAge = Convert.ToInt32(Math.Max(0, SavingsForIthStep(stepIndex)));
                person.PrivatePensionPotCombinedAtPrivatePensionAge = Convert.ToInt32(PrivatePensionPotForIthStep(stepIndex));
                person.PrivatePensionPotAtPrivatePensionAge = Convert.ToInt32(person.PrimarySteps.Steps[stepIndex].PrivatePensionAmount);
            }

            return privatePensionSet;
        }

        private decimal SavingsForIthStep(int i)
        {
            return Persons.Select(p => p.PrimarySteps.Steps[i].Savings).Sum();
        }
        
        private decimal PrivatePensionPotForIthStep(int i)
        {
            return Persons.Select(p => p.PrimarySteps.Steps[i].PrivatePensionAmount).Sum();
        }
        
        public void BalanceSavings()
        {
            var calcMinSavings = Persons.Select(p => p.CalcMinimumSteps.CurrentStep.Savings).Sum();
            var targetSavings = Persons.Select(p => p.TargetSteps.CurrentStep.Savings).Sum();
            
            foreach (var person in Persons)
            {
                person.CalcMinimumSteps.SetSavings(calcMinSavings / Persons.Count);
                person.TargetSteps.SetSavings(targetSavings / Persons.Count);
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