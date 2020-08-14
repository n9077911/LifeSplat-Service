using System;
using System.Collections.Generic;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementReport : IRetirementReport
    {
        private const decimal Monthly = 12m;
        
        public RetirementReport(IPensionAgeCalc pensionAgeCalc, PersonStatus personStatus)
        {
            TimeToRetirement = new DateAmount(DateTime.MinValue, DateTime.MinValue); //null object pattern
            Steps = new List<Step>();
            
            StatePensionDate = pensionAgeCalc.StatePensionDate(personStatus.Dob, personStatus.Sex);
            PrivatePensionDate = pensionAgeCalc.PrivatePensionDate(StatePensionDate);
            var taxResult = new IncomeTaxCalculator().TaxFor(personStatus.Salary*(1-personStatus.EmployeeContribution));
            MonthlyAfterTaxSalary = taxResult.Remainder / Monthly;
            MonthlySpending = personStatus.Spending / Monthly;
            
            AfterTaxSalary = Convert.ToInt32(taxResult.Remainder*(1-personStatus.EmployeeContribution));
            NationalInsuranceBill = Convert.ToInt32(taxResult.NationalInsurance);
            IncomeTaxBill = Convert.ToInt32(taxResult.IncomeTax);
        }

        public decimal MonthlyAfterTaxSalary { get; }
        public decimal MonthlySpending { get; }
        
        public DateTime StatePensionDate { get; set; }
        public DateTime PrivatePensionDate { get; set; }
        public int NationalInsuranceBill { get; set; }
        public int IncomeTaxBill { get; set; }
        public DateTime MinimumPossibleRetirementDate { get; set; }
        public int MinimumPossibleRetirementAge { get; set; }
        public DateTime? TargetRetirementDate { get; set; }
        public int? TargetRetirementAge { get; set; }
        public int StateRetirementAge { get; set; }
        public int PrivateRetirementAge { get; set; }
        public int AnnualStatePension { get; set; }
        public int QualifyingStatePensionYears { get; set; }
        public int AfterTaxSalary { get; set; }
        public int Spending { get; set; }
        public DateAmount TimeToRetirement { get; set; }
        public DateTime BankruptDate { get; set; } = DateTime.MaxValue;
        public List<Step> Steps { get; }
        public int PrivatePensionPot { get; set; }
        public int SavingsAtPrivatePensionAge { get; set; }
        public int SavingsAtStatePensionAge { get; set; }
        public int SavingsAtMinimumPossiblePensionAge { get; set; }
        public int SavingsAt100 { get; set; }
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