using System;
using Calculator.Input;
using Calculator.TaxSystem;

namespace Calculator.StatePensionCalculator
{
    
    public interface IStatePensionAmountCalculator
    {
        StatePensionResult Calculate(Person person, DateTime futureDate);
    }

    ///https://www.nidirect.gov.uk/articles/understanding-and-qualifying-new-state-pension
    public class StatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        private readonly IDateProvider _dateProvider;
        private readonly ITaxSystem _taxSystem;
        private readonly int _maxContributingYears;

        public StatePensionAmountCalculator(IDateProvider dateProvider, ITaxSystem taxSystem)
        {
            _dateProvider = dateProvider;
            _taxSystem = taxSystem;
            _maxContributingYears = 35;
        }

        public StatePensionResult Calculate(Person person, DateTime futureDate)
        {
            var contributingYears = NiContributingYearsCalc.CalculateContributingYears(person, futureDate, _dateProvider.Now(), _taxSystem);
            
            var cappedContributingYears = Math.Min(contributingYears, _maxContributingYears);

            var amount = cappedContributingYears < 10 ? 0 : decimal.Round((175.20m / _maxContributingYears) * cappedContributingYears * 52, 2);
            return new StatePensionResult(contributingYears, amount);
        }
    }
}