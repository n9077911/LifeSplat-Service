using System;

namespace TaxCalculator
{
    public static class AgeCalc
    {
        public static int Age(DateTime dob, DateTime onDate)
        {
            if(dob.Month < onDate.Month || (dob.Month == onDate.Month  && dob.Day <= onDate.Day))
                return onDate.Year - dob.Year;
            return onDate.Year - dob.Year - 1;
        }
    }
}