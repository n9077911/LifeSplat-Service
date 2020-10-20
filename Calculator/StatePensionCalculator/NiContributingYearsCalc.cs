using System;
using System.Linq;
using Calculator.Input;
using Calculator.TaxSystem;

namespace Calculator.StatePensionCalculator
{
    public static class NiContributingYearsCalc
    {
        public static int CalculateContributingYears(Person person, DateTime futureDate, DateTime now, ITaxSystem taxSystem)
        {
            int contributingYears;
            if (person.NiContributingYears.HasValue)
            {
                var niYEarsFromEmployment = (person.Salary > taxSystem.LowerEarningsLimit ? now.WholeYearsUntil(futureDate) : 0);
                var niYEarsFromChildCredit = 0;

                if (person.Children.Count > 0)
                {
                    var oldestDob = person.Children.Min();
                    var age = AgeCalc.Age(oldestDob, now);
                    niYEarsFromChildCredit = Math.Max(12 - age, 0);
                }
                contributingYears = person.NiContributingYears.Value + Math.Max(niYEarsFromEmployment, niYEarsFromChildCredit);
            }
            else
            {
                if (person.Salary <= taxSystem.LowerEarningsLimit)
                    contributingYears = 0;
                else
                    contributingYears = AgeCalc.Age(person.Dob, futureDate) - 21;
                
                if(person.Children.Count > 0)
                    contributingYears = Math.Max(12, contributingYears);
            }

            return contributingYears;
        }
    }
}