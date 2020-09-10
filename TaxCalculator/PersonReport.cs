using System;
using System.Collections.Generic;
using System.Linq;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class PersonReport
    {
        private readonly DateTime? _givenRetirementDate;
        private const decimal Monthly = 12;

        public PersonReport(IPensionAgeCalc pensionAgeCalc, IIncomeTaxCalculator incomeTaxCalculator, PersonStatus person, DateTime now, bool targetDateGiven, IAssumptions assumptions,
            DateTime? givenRetirementDate, decimal monthlySpendingAt)
        {
            _givenRetirementDate = givenRetirementDate;
            Status = person;
            StatePensionDate = pensionAgeCalc.StatePensionDate(person.Dob, person.Sex);
            PrivatePensionDate = pensionAgeCalc.PrivatePensionDate(StatePensionDate);
            var salaryAfterDeductions = person.Salary * (1 - person.EmployeeContribution);
            var taxResult = incomeTaxCalculator.TaxFor(salaryAfterDeductions);
            MonthlySalaryAfterDeductionsAndTax = taxResult.Remainder / Monthly;
            MonthlySalaryAfterDeductions = salaryAfterDeductions / Monthly;

            AfterTaxSalary = Convert.ToInt32(taxResult.Remainder * (1 - person.EmployeeContribution));
            NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);

            CalcMinimumSteps = new StepsReport(person, StepType.CalcMinimum, now, assumptions, monthlySpendingAt, PrivatePensionDate);
            TargetSteps = new StepsReport(person, StepType.GivenDate, now, assumptions, monthlySpendingAt, PrivatePensionDate);
            PrimarySteps = targetDateGiven ? TargetSteps : CalcMinimumSteps;
            StepReports = new List<StepsReport> {CalcMinimumSteps, TargetSteps};
        }

        public PersonStatus Status { get; }

        public List<StepsReport> StepReports { get; }

        public StepsReport CalcMinimumSteps { get; }
        public StepsReport TargetSteps { get; }
        //Between calc minimum and target retirement date steps... which are considered primary? i.e. which does the user want to see.
        public StepsReport PrimarySteps { get; }


        public decimal MonthlySalaryAfterDeductionsAndTax { get; }
        public decimal MonthlySalaryAfterDeductions { get; }
        public int NationalInsuranceBill { get; }
        public int IncomeTaxBill { get; }
        public int AfterTaxSalary { get; }

        public DateTime StatePensionDate { get; set; }
        public DateTime PrivatePensionDate { get; set; }
        public DateTime PrivateRetirementDate => _givenRetirementDate ?? PrivatePensionDate;
        public int BankruptAge { get; set; }
        public int StatePensionAge { get; set; }
        public int PrivatePensionAge { get; set; }
        public int AnnualStatePension { get; set; }
        public int NiContributingYears { get; set; }
        public int PrivatePensionPotCombinedAtPrivatePensionAge { get; set; }
        public int PrivatePensionPotCombinedAtStatePensionAge { get; set; }
        public int PrivatePensionSafeWithdrawal { get; set; }

        public DateTime MinimumPossibleRetirementDate { get; set; }
        public int MinimumPossibleRetirementAge => AgeCalc.Age(Status.Dob, MinimumPossibleRetirementDate);
        public int SavingsAtMinimumPossiblePensionAge { get; set; }
        public int SavingsCombinedAtPrivatePensionAge { get; set; }
        public int SavingsCombinedAtStatePensionAge { get; set; }
        public int SavingsAt100 { get; set; }
        public int PrivatePensionPotAtPrivatePensionAge { get; set; }
    }
}