using System;
using System.Linq;
using Calculator.Input;
using Calculator.TaxSystem;

namespace Calculator.StatePensionCalculator
{
    public static class NiContributingYearsCalc
    {
        public static int CalculateContributingYearsSoFar(Person person, Money monthlySalary, DateTime now, ITaxSystem taxSystem)
        {
            int contributingYearsFromEmployment = 0;
            int contributingYearsFromChildren = 0;
            if (person.NiContributingYears.HasValue)
            {
                return person.NiContributingYears.Value;
            }
            else
            {
                if (monthlySalary * 12 <= taxSystem.LowerEarningsLimit)
                    contributingYearsFromEmployment = 0;
                else
                    contributingYearsFromEmployment = AgeCalc.Age(person.Dob, now) - 21;

                if (person.Children.Count > 0)
                {
                    var age = EldestChildAge(person, now);
                    contributingYearsFromChildren = Math.Max(12, age);
                }
            }

            return Math.Max(contributingYearsFromEmployment, contributingYearsFromChildren);
        }

        public static bool IsThisAContributingDate(DateTime now, in DateTime futureDate, Money monthlySalary, Person person, ITaxSystem taxSystem)
        {
            // if (futureDate.Year > now.Year && futureDate.Month == now.Month) //increment every whole year
            if (person.Dob.AddMonths(1).Month == futureDate.Month) //increment every year after the persons birthday (doing it based on DOB is a simplification : correcting it would require significant test updates.
                if ((monthlySalary * 12) > taxSystem.LowerEarningsLimit || EldestChildUnder12(futureDate, person))
                    return true;

            return false;
        }

        private static bool EldestChildUnder12(DateTime now, Person person)
        {
            if (person.Children.Count == 0)
                return false;
            return EldestChildAge(person, now) < 12;
        }

        private static int EldestChildAge(Person person, DateTime futureDate)
        {
            var dob = person.Children.OrderBy(time => time).First();
            var age = AgeCalc.Age(dob, futureDate);
            return age;
        }
    }
}