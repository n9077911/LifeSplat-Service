using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementReport : IRetirementReport
    {
        private readonly IAssumptions _assumptions;

        public RetirementReport(IPensionAgeCalc pensionAgeCalc, FamilyStatus family, DateTime now, DateTime? givenRetirementDate, IAssumptions assumptions)
        {
            _assumptions = assumptions;
            TimeToRetirement = new DateAmount(DateTime.MinValue, DateTime.MinValue); //null object pattern
            foreach (var person in family.Persons)
                Persons.Add(new PersonReport(pensionAgeCalc, person, now, givenRetirementDate.HasValue, _assumptions, givenRetirementDate));

            Spending = family.Spending;
        }

        public int Spending { get; }
        public DateAmount TimeToRetirement { get; set; }
        public DateTime BankruptDate { get; private set; } = DateTime.MaxValue;

        public DateTime MinimumPossibleRetirementDate { get; set; }
        public int MinimumPossibleRetirementAge { get; set; }
        public DateTime? TargetRetirementDate { get; set; }
        public int? TargetRetirementAge { get; set; }
        public int SavingsAtMinimumPossiblePensionAge { get; set; }
        public int SavingsAtPrivatePensionAge { get; private set; }
        public int SavingsAtStatePensionAge { get; private set; }
        public int SavingsAt100 { get; private set; }

        public int PrivatePensionPotAtPrivatePensionAge { get; private set; }
        public int PrivatePensionPotAtStatePensionAge { get; private set; }

        public PersonReport PrimaryPerson => Persons.First();
        

        public List<PersonReport> Persons { get; } = new List<PersonReport>();

        public void UpdatePersonResults()
        {
            foreach (var personReport in Persons)
            {
                personReport.PrivatePensionSafeWithdrawal = Convert.ToInt32(personReport.PrivatePensionPotAtPrivatePensionAge * _assumptions.AnnualGrowthRate); 
                personReport.AnnualStatePension = Convert.ToInt32(personReport.PrimarySteps.Steps.Last().PredictedStatePensionAnnual);
                personReport.StatePensionDate = personReport.StatePensionDate;
                personReport.PrivatePensionDate = personReport.PrivatePensionDate;
            
                personReport.StatePensionAge = AgeCalc.Age(personReport.Status.Dob, personReport.StatePensionDate);
                personReport.PrivatePensionAge = AgeCalc.Age(personReport.Status.Dob, personReport.PrivatePensionDate);
            }
        }
        
        public void UpdateResultsBasedOnSetDates()
        {
            var (privatePensionSet, statePensionSet, bankrupt) = (false, false, false);
            
            for (int i = 0; i < PrimaryPerson.CalcMinimumSteps.Steps.Count; i++)
            {
                var stepDate = PrimaryPerson.CalcMinimumSteps.Steps[i].Date;
                if (Persons.Select(p => p.PrimarySteps.Steps[i].Savings).Sum() < 0 && !bankrupt)
                {
                    bankrupt = true;
                    BankruptDate = stepDate;
                }

                if (stepDate >= PrimaryPerson.PrivatePensionDate && !privatePensionSet)
                {
                    privatePensionSet = true;
                    SavingsAtPrivatePensionAge = Convert.ToInt32(SavingsForIthStep(i));
                    PrivatePensionPotAtPrivatePensionAge = Convert.ToInt32(PrivatePensionPotForIthStep(i));
                    foreach (var person in Persons)
                        person.PrivatePensionPotAtPrivatePensionAge = Convert.ToInt32(person.PrimarySteps.Steps[i].PrivatePensionAmount);
                }

                if (stepDate >= PrimaryPerson.StatePensionDate && !statePensionSet)
                {
                    statePensionSet = true;
                    SavingsAtStatePensionAge = Convert.ToInt32(SavingsForIthStep(i));
                    PrivatePensionPotAtStatePensionAge = Convert.ToInt32(PrivatePensionPotForIthStep(i));
                }
            }

            SavingsAt100 = Convert.ToInt32(SavingsForIthStep(PrimaryPerson.CalcMinimumSteps.Steps.Count-1));
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
}