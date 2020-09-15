using System;
using Calculator.Input;

namespace Calculator.StatePensionCalculator
{
    public class FixedStatePensionAmountCalculator : IStatePensionAmountCalculator
    {
        private readonly decimal _amount;

        public FixedStatePensionAmountCalculator(decimal amount)
        {
            _amount = amount;
        }

        public StatePensionResult Calculate(Person person, DateTime futureDate)
        {
            return new StatePensionResult(35, _amount);
        }
    }
}