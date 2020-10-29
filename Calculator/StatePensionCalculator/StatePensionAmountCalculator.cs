using System;

namespace Calculator.StatePensionCalculator
{
    
    public interface IStatePensionAmountCalculator
    {
        StatePensionResult Calculate(int niContributingYears);
    }

    ///https://www.nidirect.gov.uk/articles/understanding-and-qualifying-new-state-pension
    public class StatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        private readonly int _maxContributingYears;

        public StatePensionAmountCalculator()
        {
            _maxContributingYears = 35;
        }

        public StatePensionResult Calculate(int niContributingYears)
        {
            var cappedContributingYears = Math.Min(niContributingYears, _maxContributingYears);

            var amount = cappedContributingYears < 10 ? 0 : decimal.Round((175.20m / _maxContributingYears) * cappedContributingYears * 52, 2);
            return new StatePensionResult(niContributingYears, amount);
        }
    }
}