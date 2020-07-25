using System;
using System.Globalization;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementCalculator : IRetirementCalculator
    {
        private double _safeWithdrawalRate = 0.04;
        private int _monthly = 12;
        private DateTime _now;

        public RetirementCalculator(IDateProvider dateProvider)
        {
            _now = dateProvider.Now();
        }

        public IRetirementReport ReportFor(PersonStatus personStatus)
        {
            var result = new RetirementReport();

            result.TargetSavings = CalcTargetSavingsRequirement(personStatus);
            result.RetirementDate = CalcRetirementDate(personStatus, result);
            result.RetirementAge = AgeCalc.Age(personStatus.Dob, result.RetirementDate);
            
            result.TimeToRetirement = new DateAmount(_now, result.RetirementDate);

            return result;
        }

        private DateTime CalcRetirementDate(PersonStatus personStatus, RetirementReport retirementReport)
        {
            var afterTaxSalary = new IncomeTaxCalculator().TaxFor(personStatus.Salary).Remainder;
            var monthlySpareCash = (afterTaxSalary - personStatus.Spending) / _monthly;
            var months = (int) Math.Ceiling(retirementReport.TargetSavings / monthlySpareCash);

            var retirementReportRetirementDate = _now.AddMonths(months);
            return retirementReportRetirementDate;
        }

        private int CalcTargetSavingsRequirement(PersonStatus personStatus)
        {
            var target = personStatus.Spending / _safeWithdrawalRate;
            var ceiling = (int)Math.Ceiling(target);
            return ceiling;
        }
    }

    public class AgeCalc
    {
        public static int Age(DateTime dob, DateTime onDate)
        {
            if(dob.Month < onDate.Month || (dob.Month == onDate.Month  && dob.Day <= onDate.Day))
                return onDate.Year - dob.Year;
            return onDate.Year - dob.Year - 1;
        }
    }
}