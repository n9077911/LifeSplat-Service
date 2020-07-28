using System;
using System.Collections.Generic;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public class RetirementReport : IRetirementReport
    {
        public RetirementReport()
        {
            TimeToRetirement = new DateAmount(DateTime.Now, DateTime.Now); //null object pattern
            Steps = new List<Step>();
        }

        public DateTime StateRetirementDate { get; set; }
        public DateTime RetirementDate { get; set; }
        public int TargetSavings { get; set; }
        public int RetirementAge { get; set; }
        public int StateRetirementAge { get; set; }
        public decimal StatePensionAmount { get; set; }
        public DateAmount TimeToRetirement { get; set; }
        public List<Step> Steps { get; set; }
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
            return $"{Years} Years and {Months} Months";
        }
    }
}