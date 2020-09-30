using System;
using System.Collections.Generic;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.StatePensionCalculator;

namespace Calculator.Output
{
    internal class PersonReport : IPersonReport
    {
        private const decimal Monthly = 12;
        private bool taken25 = false;
        private bool take25 = false;

        public PersonReport(IPensionAgeCalc pensionAgeCalc, IIncomeTaxCalculator incomeTaxCalculator, Person person, DateTime now, bool targetDateGiven, IAssumptions assumptions, decimal monthlySpendingAt)
        {
            Person = person;
            StatePensionDate = pensionAgeCalc.StatePensionDate(person.Dob, person.Sex);
            PrivatePensionDate = pensionAgeCalc.PrivatePensionDate(StatePensionDate);
            var salaryAfterDeductions = person.Salary * (1 - person.EmployeeContribution);
            var taxResult = incomeTaxCalculator.TaxFor(salaryAfterDeductions);
            MonthlySalaryAfterDeductions = salaryAfterDeductions / Monthly;

            AfterTaxSalary = Convert.ToInt32(taxResult.AfterTaxIncome * (1 - person.EmployeeContribution));
            NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);

            StepReport = targetDateGiven 
                ? new StepsReport(person, StepType.GivenDate, now, assumptions, monthlySpendingAt, PrivatePensionDate) 
                : new StepsReport(person, StepType.CalcMinimum, now, assumptions, monthlySpendingAt, PrivatePensionDate);

            take25 = assumptions.Take25;
        }

        public Person Person { get; }

        public StepsReport StepReport { get; }

        public decimal MonthlySalaryAfterDeductions { get; }
        public int NationalInsuranceBill { get; }
        public int IncomeTaxBill { get; }
        public int AfterTaxSalary { get; }

        public DateTime StatePensionDate { get; set; }
        public DateTime PrivatePensionDate { get; set; }
        public int BankruptAge { get; set; }
        public int StatePensionAge { get; set; }
        public int PrivatePensionAge { get; set; }
        public int AnnualStatePension { get; set; }
        public int NiContributingYears { get; set; }
        public int PrivatePensionPotCombinedAtPrivatePensionAge { get; set; }
        public int PrivatePensionPotCombinedAtStatePensionAge { get; set; }
        public int PrivatePensionSafeWithdrawal { get; set; }

        public DateTime MinimumPossibleRetirementDate { get; set; }
        public int MinimumPossibleRetirementAge => AgeCalc.Age(Person.Dob, MinimumPossibleRetirementDate);
        public int SavingsAtMinimumPossiblePensionAge { get; set; }
        public int SavingsCombinedAtPrivatePensionAge { get; set; }
        public int SavingsCombinedAtStatePensionAge { get; set; }
        public int PrivatePensionPotAtPrivatePensionAge { get; set; }
        public void UpdateMinimumPossibleRetirementDate(in DateTime minimumPossibleRetirementDate)
        {
            MinimumPossibleRetirementDate = minimumPossibleRetirementDate;
        }

        public bool Take25WhenRetired(in bool calcdMinimum, in DateTime now, DateTime? givenRetirementDate)
        {
            if (take25 && !taken25 && 
                ((calcdMinimum && now > PrivatePensionDate) 
                || (givenRetirementDate != null && now > givenRetirementDate  && now > PrivatePensionDate)))
            {
                taken25 = true;
                return true;
            }

            return false;
        }
    }
}