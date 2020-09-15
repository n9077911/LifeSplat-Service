using System;

namespace TaxCalculator
{
    public static class AgeCalc
    {
        public static int Age(DateTime dob, DateTime onDate)
        {
            return dob.WholeYearsUntil(onDate);
        }
    }
}