using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementReport : IRetirementReport
    {
        private readonly FamilyStatus _family;
        private const decimal Monthly = 12m;

        public RetirementReport(IPensionAgeCalc pensionAgeCalc, FamilyStatus family)
        {
            _family = family;
            TimeToRetirement = new DateAmount(DateTime.MinValue, DateTime.MinValue); //null object pattern
            StepsDict = new Dictionary<PersonStatus, List<Step>>();
            PersonReports = new Dictionary<PersonStatus, PersonReport>();
            foreach (var person in family.Persons)
            {
                StepsDict.Add(person, new List<Step>());
                var personReport = new PersonReport(pensionAgeCalc, person);
                PersonReports.Add(person, personReport);
            }

            MonthlySpending = family.Spending / Monthly;
        }

        public decimal MonthlySpending { get; }
        public int Spending { get; set; }
        public DateAmount TimeToRetirement { get; set; }
        public DateTime BankruptDate { get; set; } = DateTime.MaxValue;
        public List<Step> Steps => StepsDict[_family.PrimaryPerson];
        public Dictionary<PersonStatus, List<Step>> StepsDict { get; }
        public Dictionary<PersonStatus, PersonReport> PersonReports { get; }

        public DateTime MinimumPossibleRetirementDate { get; set; }
        public int MinimumPossibleRetirementAge { get; set; }
        public DateTime? TargetRetirementDate { get; set; }
        public int? TargetRetirementAge { get; set; }
        public int SavingsAtPrivatePensionAge { get; set; }
        public int SavingsAtStatePensionAge { get; set; }
        public int SavingsAtMinimumPossiblePensionAge { get; set; }
        public int SavingsAt100 { get; set; }
        public PersonReport PrimaryPerson => PersonReports[_family.PrimaryPerson];

        public void CurrentSteps(FamilyStatus family, bool targetDateGiven)
        {
            foreach (var person in family.Persons)
                StepsDict[person].Add(targetDateGiven ? person.TargetSteps.CurrentStep : person.CalcMinimumSteps.CurrentStep);
        }

        public void UpdatePersonResults()
        {
            foreach (var person in _family.Persons)
            {
                var personReport = PersonReports[person];
                personReport.PrivatePensionSafeWithdrawal = Convert.ToInt32(personReport.PrivatePensionPot * 0.04); //todo: refactor 
                personReport.AnnualStatePension = Convert.ToInt32(StepsDict[person].Last().PredictedStatePensionAnnual);
                personReport.StatePensionDate = person.StatePensionDate;
                personReport.PrivatePensionDate = person.PrivatePensionDate;
            
                personReport.StateRetirementAge = AgeCalc.Age(person.Dob, person.StatePensionDate);
                personReport.PrivateRetirementAge = AgeCalc.Age(person.Dob, person.PrivatePensionDate);
            }
        }
        
        public void UpdateResultsBasedOnSetDates()
        {
            var (privatePensionSet, statePensionSet, bankrupt) = (false, false, false);
            for (int i = 0; i < _family.PrimaryPerson.CalcMinimumSteps.Steps.Count; i++)
            {
                var stepDate = _family.PrimaryPerson.CalcMinimumSteps.Steps[i].Date;
                if (StepsDict.Values.Select(list => list[i].Savings).Sum() < 0 && !bankrupt)
                {
                    bankrupt = true;
                    BankruptDate = stepDate;
                }

                if (stepDate >= _family.PrimaryPerson.PrivatePensionDate && !privatePensionSet) //TODO: assumes does not work past private pension date 
                {
                    privatePensionSet = true;
                    SavingsAtPrivatePensionAge = Convert.ToInt32(SavingsForIthStep(i));
                }

                foreach (var person in _family.Persons)
                    if (stepDate >= person.PrivatePensionDate && !PersonReports[person].PrivatePensionPot.HasValue)
                        PersonReports[person].PrivatePensionPot = Convert.ToInt32(StepsDict[person][i].PrivatePensionAmount);

                if (stepDate >= _family.PrimaryPerson.StatePensionDate && !statePensionSet)
                {
                    //TODO: FIX! in middle of another refactor but below should be statePensionSet!
                    privatePensionSet = true;
                    SavingsAtStatePensionAge = Convert.ToInt32(SavingsForIthStep(i));
                }
            }

            SavingsAt100 = Convert.ToInt32(SavingsForIthStep(_family.PrimaryPerson.CalcMinimumSteps.Steps.Count-1));
        }

        private decimal SavingsForIthStep(int i)
        {
            return StepsDict.Values.Select(list => list[i].Savings).Sum();
        }
    }

    public class PersonReport
    {
        private const int Monthly = 12;

        public PersonReport(IPensionAgeCalc pensionAgeCalc, PersonStatus person)
        {
            StatePensionDate = pensionAgeCalc.StatePensionDate(person.Dob, person.Sex);
            PrivatePensionDate = pensionAgeCalc.PrivatePensionDate(StatePensionDate);
            var taxResult = new IncomeTaxCalculator().TaxFor(person.Salary * (1 - person.EmployeeContribution));
            MonthlyAfterTaxSalary = taxResult.Remainder / Monthly;

            AfterTaxSalary = Convert.ToInt32(taxResult.Remainder * (1 - person.EmployeeContribution));
            NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);
        }

        public decimal MonthlyAfterTaxSalary { get; }
        public DateTime StatePensionDate { get; set; }
        public DateTime PrivatePensionDate { get; set; }
        public int NationalInsuranceBill { get; set; }
        public int IncomeTaxBill { get; set; }
        public int StateRetirementAge { get; set; }
        public int PrivateRetirementAge { get; set; }
        public int AnnualStatePension { get; set; }
        public int QualifyingStatePensionYears { get; set; }
        public int AfterTaxSalary { get; set; }
        public int? PrivatePensionPot { get; set; } = null;
        public int PrivatePensionSafeWithdrawal { get; set; }
    }

    //An amount of time specified in years, month and days
    public class DateAmount
    {
        public DateAmount(DateTime dateStart, DateTime dateEnd)
        {
            Years = dateEnd.Year - dateStart.Year;
            Months = dateEnd.Month - dateStart.Month;
            if (dateStart.Month > dateEnd.Month)
            {
                Years -= 1;
                Months += 12;
            }
        }

        private int Years { get; }
        private int Months { get; }

        public int TotalMonths()
        {
            return (Years * 12) + Months;
        }

        public override string ToString()
        {
            var pluralYears = Years == 1 ? "" : "s";
            var pluralMonths = Months == 1 ? "" : "s";
            return $"{Years} Year{pluralYears} and {Months} Month{pluralMonths}";
        }
    }
}