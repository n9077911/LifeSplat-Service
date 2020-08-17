using System;
using System.Collections.Generic;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class PersonReport
    {
        private const int Monthly = 12;

        public PersonReport(IPensionAgeCalc pensionAgeCalc, PersonStatus person, DateTime now, bool targetDateGiven, IAssumptions assumptions)
        {
            Status = person;
            StatePensionDate = pensionAgeCalc.StatePensionDate(person.Dob, person.Sex);
            PrivatePensionDate = pensionAgeCalc.PrivatePensionDate(StatePensionDate);
            var taxResult = new IncomeTaxCalculator().TaxFor(person.Salary * (1 - person.EmployeeContribution));
            MonthlyAfterTaxSalary = taxResult.Remainder / Monthly;

            AfterTaxSalary = Convert.ToInt32(taxResult.Remainder * (1 - person.EmployeeContribution));
            NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);

            CalcMinimumSteps = new StepsReport(person, StepType.CalcMinimum, now, assumptions); 
            TargetSteps = new StepsReport(person, StepType.GivenDate, now, assumptions);
            PrimarySteps = targetDateGiven ? TargetSteps : CalcMinimumSteps;
            StepDescriptions = new List<StepsReport> {CalcMinimumSteps, TargetSteps};
        }

        public PersonStatus Status { get; }

        public List<StepsReport> StepDescriptions { get; }

        public StepsReport CalcMinimumSteps { get; }
        public StepsReport TargetSteps { get; }
        public StepsReport PrimarySteps { get; }

        
        public decimal MonthlyAfterTaxSalary { get; }
        public int NationalInsuranceBill { get; }
        public int IncomeTaxBill { get; }
        public int AfterTaxSalary { get; }

        public DateTime StatePensionDate { get; set; }
        public DateTime PrivatePensionDate { get; set; }
        public int StateRetirementAge { get; set; }
        public int PrivateRetirementAge { get; set; }
        public int AnnualStatePension { get; set; }
        public int QualifyingStatePensionYears { get; set; }
        public int? PrivatePensionPot { get; set; }
        public int PrivatePensionSafeWithdrawal { get; set; }
    }
}