using System;

namespace TaxCalculator
{
    ///An amount of time specified in years and month
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