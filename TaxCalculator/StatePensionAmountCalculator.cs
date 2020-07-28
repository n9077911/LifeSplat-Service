using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public interface IStatePensionAmountCalculator
    {
        decimal Calculate(PersonStatus personStatus, DateTime date);
    }

    ///https://www.nidirect.gov.uk/articles/understanding-and-qualifying-new-state-pension
    public class StatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        public decimal Calculate(PersonStatus personStatus, DateTime date)
        {
            var age = AgeCalc.Age(personStatus.Dob, date);
            var contributingYears = Math.Min(age-21, 35);
            
            if (contributingYears < 10)
                return 0;
            return decimal.Round(((175.20m/35) * contributingYears) * 52, 2);
        }
    }

    public class FixedStatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        private readonly decimal _amount;

        public FixedStatePensionAmountCalculator(decimal amount)
        {
            _amount = amount;
        }

        public decimal Calculate(PersonStatus personStatus, DateTime date)
        {
            return _amount;
        }
    }
}