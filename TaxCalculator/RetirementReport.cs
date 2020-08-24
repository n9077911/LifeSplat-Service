﻿
using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementReport : IRetirementReport
    {
        private readonly IAssumptions _assumptions;

        public RetirementReport(IPensionAgeCalc pensionAgeCalc, IIncomeTaxCalculator incomeTaxCalculator, FamilyStatus family, DateTime now, DateTime? givenRetirementDate, IAssumptions assumptions)
        {
            _assumptions = assumptions;
            TimeToRetirement = new DateAmount(DateTime.MinValue, DateTime.MinValue); //null object pattern
            foreach (var person in family.Persons)
                Persons.Add(new PersonReport(pensionAgeCalc, incomeTaxCalculator, person, now, givenRetirementDate.HasValue, _assumptions, givenRetirementDate));

            Spending = family.Spending;
        }

        public int Spending { get; }
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
        public int SavingsAt100 { get; set; }



        public PersonReport PrimaryPerson => Persons.First();
        

        public List<PersonReport> Persons { get; } = new List<PersonReport>();
        public int MonthlySpending => Spending / 12;

        public void UpdatePersonResults()
        {
            foreach (var personReport in Persons)
            {
                personReport.PrivatePensionSafeWithdrawal = Convert.ToInt32(personReport.PrivatePensionPotAtPrivatePensionAge * _assumptions.AnnualGrowthRate); 
                personReport.AnnualStatePension = Convert.ToInt32(personReport.PrimarySteps.Steps.Last().PredictedStatePensionAnnual);
                personReport.NiContributingYears = personReport.PrimarySteps.Steps.Last().NiContributingYears;
                personReport.StatePensionDate = personReport.StatePensionDate;
                personReport.PrivatePensionDate = personReport.PrivatePensionDate;
            
                personReport.BankruptAge = AgeCalc.Age(personReport.Status.Dob, BankruptDate);
                personReport.StatePensionAge = AgeCalc.Age(personReport.Status.Dob, personReport.StatePensionDate);
                personReport.PrivatePensionAge = AgeCalc.Age(personReport.Status.Dob, personReport.PrivatePensionDate);
            }
        }
        
        public void UpdateResultsBasedOnSetDates()
        {
            var bankrupt = false;
            var detailsSet = Persons.Select(p => new Details{Person = p, PrivatePensionSet = false, StatePensionSet=false}).ToList();
            
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

        private bool UpdateStatePensionDetails(PersonReport person, DateTime stepDate, bool statePensionSet, int stepIndex)
        {
            if (stepDate >= person.StatePensionDate && !statePensionSet)
            {
                statePensionSet = true;
                person.SavingsCombinedAtStatePensionAge = Convert.ToInt32(Math.Max(0, SavingsForIthStep(stepIndex)));
                person.PrivatePensionPotCombinedAtStatePensionAge = Convert.ToInt32(PrivatePensionPotForIthStep(stepIndex));
            }

            return statePensionSet;
        }

        private bool UpdatePrivatePensionDetails(PersonReport person, DateTime stepDate, bool privatePensionSet, int stepIndex)
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
                person.CalcMinimumSteps.SetSavings(calcMinSavings / Persons.Count());
                person.TargetSteps.SetSavings(targetSavings / Persons.Count());
            }
        }
    }

    public class Details
    {
        public PersonReport Person { get; set; }
        public bool PrivatePensionSet { get; set; }
        public bool StatePensionSet { get; set; }
    }
}