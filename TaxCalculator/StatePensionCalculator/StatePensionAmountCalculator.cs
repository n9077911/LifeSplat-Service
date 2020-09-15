using System;
using TaxCalculator.ExternalInterface;
using TaxCalculator.Input;
using TaxCalculator.StatePensionCalculator;
using TaxCalculator.TaxSystem;

namespace TaxCalculator
{
    
    public interface IStatePensionAmountCalculator
    {
        StatePensionResult Calculate(Person person, DateTime futureDate);
    }

    ///https://www.nidirect.gov.uk/articles/understanding-and-qualifying-new-state-pension
    public class StatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        private readonly DateTime _now;
        private readonly int _maxContributingYears;
        private readonly int _lowerEarningsLimit;

        public StatePensionAmountCalculator(IDateProvider dateProvider, ITaxSystem taxSystem)
        {
            _now = dateProvider.Now();
            _maxContributingYears = 35;
            _lowerEarningsLimit = taxSystem.LowerEarningsLimit;
        }

        public StatePensionResult Calculate(Person person, DateTime futureDate)
        {
            var contributingYears = person.NiContributingYears.HasValue
                ? person.NiContributingYears.Value + (person.Salary > _lowerEarningsLimit ? _now.WholeYearsUntil(futureDate) : 0)
                : (person.Salary > _lowerEarningsLimit ? AgeCalc.Age(person.Dob, futureDate) - 21 : 0);

            var cappedContributingYears = Math.Min(contributingYears, _maxContributingYears);

            var amount = cappedContributingYears < 10 ? 0 : decimal.Round((175.20m / _maxContributingYears) * cappedContributingYears * 52, 2);
            return new StatePensionResult(contributingYears, amount);
        }
    }
}