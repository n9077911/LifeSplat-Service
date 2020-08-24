using System;
using TaxCalculator.ExternalInterface;
using TaxCalculator.TaxSystem;

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
        private int _maxContributingYears;
        private int _lowerEarningsLimit;

        public StatePensionAmountCalculator(IDateProvider dateProvider, ITaxSystem taxSystem)
        {
            _now = dateProvider.Now();
            _maxContributingYears = 35;
            _lowerEarningsLimit = taxSystem.LowerEarningsLimit;
        }

        public (int, decimal) Calculate(PersonStatus personStatus, DateTime futureDate)
        {
            var contributingYears = personStatus.NiContributingYears.HasValue
                ? personStatus.NiContributingYears.Value + (personStatus.Salary > _lowerEarningsLimit ? _now.WholeYearsUntil(futureDate) : 0)
                : (personStatus.Salary > _lowerEarningsLimit ? AgeCalc.Age(personStatus.Dob, futureDate) - 21 : 0);

            var cappedContributingYears = Math.Min(contributingYears, _maxContributingYears);

            var amount = cappedContributingYears < 10 ? 0 : decimal.Round((175.20m / _maxContributingYears) * cappedContributingYears * 52, 2);
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