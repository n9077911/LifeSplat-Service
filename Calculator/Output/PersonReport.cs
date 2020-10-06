using System;
using System.Collections.Generic;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;

namespace Calculator.Output
{
    internal class PersonReport : IPersonReport
    {
        private const decimal Monthly = 12;
        private bool _taken25 = false;
        private readonly bool _take25 = false;

        public PersonReport(IPensionAgeCalc pensionAgeCalc, IIncomeTaxCalculator incomeTaxCalculator, Person person, DateTime now, bool targetDateGiven, IAssumptions assumptions, decimal monthlySpendingAt)
        {
            Person = person;
            StatePensionDate = pensionAgeCalc.StatePensionDate(person.Dob, person.Sex);
            PrivatePensionDate = pensionAgeCalc.PrivatePensionDate(StatePensionDate);
            var salaryAfterDeductions = person.Salary * (1 - person.EmployeeContribution);
            var taxResult = incomeTaxCalculator.TaxFor(salaryAfterDeductions);
            var taxResultWithRental = incomeTaxCalculator.TaxFor(salaryAfterDeductions, rentalIncome: person.RentalPortfolio.RentalIncome());
            MonthlySalaryAfterDeductions = salaryAfterDeductions / Monthly;

            NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);
            RentalTaxBill = Convert.ToInt32(taxResultWithRental.TotalTaxFor(IncomeType.RentalIncome));
            TakeHomeSalary = Convert.ToInt32(taxResult.AfterTaxIncome);
            TakeHomeRentalIncome = Convert.ToInt32(person.RentalPortfolio.TotalNetIncome() - RentalTaxBill);
            
            StepReport = targetDateGiven 
                ? new StepsReport(person, StepType.GivenDate, now, assumptions, monthlySpendingAt, PrivatePensionDate) 
                : new StepsReport(person, StepType.CalcMinimum, now, assumptions, monthlySpendingAt, PrivatePensionDate);

            _take25 = assumptions.Take25;
        }

        public Person Person { get; }

        public StepsReport StepReport { get; }

        public decimal MonthlySalaryAfterDeductions { get; }
        public int NationalInsuranceBill { get; }
        public int IncomeTaxBill { get; }
        public int RentalTaxBill { get; }
        public int TakeHomeSalary { get; }
        public int TakeHomeRentalIncome { get; }

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
            if (_take25 && !_taken25 && 
                ((calcdMinimum && now > PrivatePensionDate) 
                || (givenRetirementDate != null && now > givenRetirementDate  && now > PrivatePensionDate)))
            {
                _taken25 = true;
                return true;
            }

            return false;
        }
    }
}