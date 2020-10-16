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
        private readonly Family _family;
        private readonly IAssumptions _assumptions;

        public RetirementReport(IPensionAgeCalc pensionAgeCalc, IIncomeTaxCalculator incomeTaxCalculator, Family family, DateTime now, DateTime? givenRetirementDate, IAssumptions assumptions)
        {
            _family = family;
            _assumptions = assumptions;
            TimeToRetirement = new DateAmount(DateTime.MinValue, DateTime.MinValue); 
            TargetRetirementDate = givenRetirementDate; 
    
            var spendingStepInputs = family.SpendingStepInputs.OrderBy(input => input.Date).ToList();
            for (int i = 0; i < spendingStepInputs.Count; i++)
            {
                var endDate = i < spendingStepInputs.Count - 1 ? spendingStepInputs[i + 1].Date : family.PrimaryPerson.Dob.AddYears(100);
                SpendingSteps.Add(new SpendingStepReport(spendingStepInputs[i].Date, endDate.AddDays(-1), spendingStepInputs[i].NewAmount));
            }
            
            var monthlySpendingAt = MonthlySpendingAt(now)/family.Persons.Count;
            foreach (var person in family.Persons)
                Persons.Add(new PersonReport(pensionAgeCalc, incomeTaxCalculator, person, now, givenRetirementDate, _assumptions, monthlySpendingAt));
        }

        public List<SpendingStepReport> SpendingSteps { get; } = new List<SpendingStepReport>();
        public DateAmount TimeToRetirement { get; set; }
        public DateTime BankruptDate { get; private set; } = DateTime.MaxValue;
        
        public DateTime? TargetRetirementDate { get; set; }
        public int? TargetRetirementAge { get; set; }

        public DateTime FinancialIndependenceDate => PrimaryPerson.FinancialIndependenceDate;
        public int FinancialIndependenceAge => PrimaryPerson.FinancialIndependenceAge;
        
        public int SavingsCombinedAtFinancialIndependenceAge => Persons.Count > 1 ? PrimaryPerson.SavingsAtFinancialIndependenceAge + Persons[1].SavingsAtFinancialIndependenceAge : PrimaryPerson.SavingsAtFinancialIndependenceAge;
        public int PrivatePensionCombinedAtFinancialIndependenceAge => Persons.Count > 1 ? PrimaryPerson.PrivatePensionPotAtFinancialIndependenceAge + Persons[1].PrivatePensionPotAtFinancialIndependenceAge : PrimaryPerson.PrivatePensionPotAtFinancialIndependenceAge;
        public int? SavingsCombinedAtTargetRetirementAge => Persons.Count > 1 ? PrimaryPerson.SavingsAtTargetRetirementAge + Persons[1].SavingsAtTargetRetirementAge : PrimaryPerson.SavingsAtTargetRetirementAge;
        public int? PrivatePensionCombinedAtTargetRetirementAge => Persons.Count > 1 ? PrimaryPerson.PrivatePensionPotAtTargetRetirementAge + Persons[1].PrivatePensionPotAtTargetRetirementAge : PrimaryPerson.PrivatePensionPotAtTargetRetirementAge;
        
        public int SavingsAt100 { get; private set; }

        public IPersonReport PrimaryPerson => Persons.First();

        public List<IPersonReport> Persons { get; } = new List<IPersonReport>();

        public decimal MonthlySpendingAt(DateTime date)
        {
            var spendingStep = SpendingSteps.FirstOrDefault(step => date.Date >= step.StartDate.Date && date.Date <= step.EndDate.Date) ?? SpendingSteps.Last();
            return spendingStep.Spending / 12m;
        }

        public int CurrentSavingsRate()
        {
            var spending = (decimal)_family.SpendingStepInputs.First().NewAmount;
            var earnings = Persons.Sum(p => p.TakeHomeSalary + p.PensionContributions);
            var savings = earnings-spending;
            var oneMinusRatio = savings/earnings;
            return Convert.ToInt32(oneMinusRatio * 100);
        }

        public void UpdateFinancialIndependenceDate(DateTime financialIndependenceDate)
        {
            PrimaryPerson.UpdateFinancialIndependenceDate(financialIndependenceDate);
            Persons.Last().UpdateFinancialIndependenceDate(financialIndependenceDate);
        }

        public void ProcessResults(DateTime? givenRetirementDate, DateTime now)
        {
            UpdateResultsBasedOnSetDates();
            UpdatePersonResults();
            TimeToRetirement = new DateAmount(now, FinancialIndependenceDate);
            TargetRetirementDate = givenRetirementDate;
            TargetRetirementAge = TargetRetirementDate.HasValue ? AgeCalc.Age(PrimaryPerson.Person.Dob, TargetRetirementDate.Value) : (int?) null;
        }

        private void UpdatePersonResults()
        {
            foreach (var personReport in Persons)
            {
                personReport.PrivatePensionSafeWithdrawal = Convert.ToInt32(personReport.PrivatePensionPotAtCrystallisationAge * _assumptions.AnnualGrowthRate); 
                personReport.AnnualStatePension = Convert.ToInt32(personReport.StepReport.Steps.Last().PredictedStatePensionAnnual);
                personReport.NiContributingYears = personReport.StepReport.Steps.Last().NiContributingYears;
                personReport.StatePensionDate = personReport.StatePensionDate;
            
                personReport.BankruptAge = AgeCalc.Age(personReport.Person.Dob, BankruptDate);
            }
        }

        private void UpdateResultsBasedOnSetDates()
        {
            var bankrupt = false;
            var financialIndependenceSet = false;
            var targetRetirementSet = false;
            
            for (int stepIndex = 0; stepIndex < PrimaryPerson.StepReport.Steps.Count; stepIndex++)
            {
                var stepDate = PrimaryPerson.StepReport.Steps[stepIndex].Date;
                if (Persons.Select(p => p.StepReport.Steps[stepIndex].Investments).Sum() + Persons.Select(p => p.StepReport.Steps[stepIndex].EmergencyFund).Sum() < 0 && !bankrupt)
                {
                    bankrupt = true;
                    BankruptDate = stepDate;
                }

                if (!financialIndependenceSet && stepDate >= FinancialIndependenceDate)
                {
                    var index = stepIndex;
                    Persons.ForEach(report => UpdateFinancialIndependenceDetails(report, index));
                    financialIndependenceSet = true;
                }
                
                if (!targetRetirementSet && stepDate >= TargetRetirementDate)
                {
                    var index = stepIndex;
                    Persons.ForEach(report => UpdateTargetRetirementDateDetails(report, index));
                    targetRetirementSet = true;
                }
            }

            SavingsAt100 = Convert.ToInt32(TotalSavingsForIthStep(PrimaryPerson.StepReport.Steps.Count-1));
        }

        private void UpdateFinancialIndependenceDetails(IPersonReport person, int stepIndex)
        {
            person.SavingsAtFinancialIndependenceAge = Convert.ToInt32(Math.Max(0, TotalSavingsForIthStep(stepIndex, person)));
            person.PrivatePensionPotAtFinancialIndependenceAge = Convert.ToInt32(PrivatePensionPotForIthStep(stepIndex, person));
        }
        
        private void UpdateTargetRetirementDateDetails(IPersonReport person, int stepIndex)
        {
            person.SavingsAtTargetRetirementAge = Convert.ToInt32(Math.Max(0, TotalSavingsForIthStep(stepIndex, person)));
            person.PrivatePensionPotAtTargetRetirementAge = Convert.ToInt32(PrivatePensionPotForIthStep(stepIndex, person));
        }

        private decimal TotalSavingsForIthStep(int i, IPersonReport person = null)
        {
            if (person != null)
                return person.StepReport.Steps[i].Investments + person.StepReport.Steps[i].EmergencyFund;
            return Persons.Select(p => p.StepReport.Steps[i].Investments + p.StepReport.Steps[i].EmergencyFund).Sum();
        }
        
        private decimal PrivatePensionPotForIthStep(int i, IPersonReport person = null)
        {
            if (person != null)
                return person.StepReport.Steps[i].PrivatePensionAmount;
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

        public IPersonReport PersonReportFor(Person person)
        {
            if (person == null)
                return null;
            return Persons.First(report => report.Person == person);
        }
    }

    public interface ISpendingForDate
    {
        decimal MonthlySpendingAt(DateTime date);
    }
}