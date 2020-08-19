using System;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator
{
    public interface IStatePensionAmountCalculator
    {
        (int, decimal) Calculate(PersonStatus personStatus, DateTime futureDate);
    }

    ///https://www.nidirect.gov.uk/articles/understanding-and-qualifying-new-state-pension
    public class StatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        private readonly DateTime _now;

        public StatePensionAmountCalculator(IDateProvider dateProvider)
        {
            _now = dateProvider.Now();
        }

        public (int, decimal) Calculate(PersonStatus personStatus, DateTime futureDate)
        {
            var contributingYears = personStatus.NiContributingYears.HasValue 
                ? personStatus.NiContributingYears.Value + _now.WholeYearsUntil(futureDate) 
                : AgeCalc.Age(personStatus.Dob, futureDate) - 21;

            var cappedContributingYears = Math.Min(contributingYears, 35);

            var amount = cappedContributingYears < 10 ? 0 : decimal.Round((175.20m/35) * cappedContributingYears * 52, 2);
            return (contributingYears, amount);
        }
    }

    public class FixedStatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        private readonly decimal _amount;

        public FixedStatePensionAmountCalculator(decimal amount)
        {
            _amount = amount;
        }

        public (int, decimal) Calculate(PersonStatus personStatus, DateTime futureDate)
        {
            return (35, _amount);
        }
    }
}