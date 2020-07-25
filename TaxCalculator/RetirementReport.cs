using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementReport : IRetirementReport
    {
        public RetirementReport()
        {
            TimeToRetirement = new DateAmount(DateTime.Now, DateTime.Now); //null object pattern
        }

        public DateTime RetirementDate { get; set; }
        public int TargetSavings { get; set; }
        public int RetirementAge { get; set; }
        public int YearsToRetirement { get; set; }
        public DateAmount TimeToRetirement { get; set; }
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

        public int Years { get; }
        public int Months { get; }

        public override string ToString()
        {
            return $"{Years} Years and {Months} Months";
        }
    }
}