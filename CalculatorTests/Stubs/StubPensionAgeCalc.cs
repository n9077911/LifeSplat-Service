using System;
using Calculator;
using Calculator.StatePensionCalculator;

namespace CalculatorTests.Stubs
{
    public class StubPensionAgeCalc : IPensionAgeCalc
    {
        private readonly DateTime _statePensionAge;
        private readonly DateTime? _privatePensionAge;

        public StubPensionAgeCalc(DateTime statePensionAge, DateTime? privatePensionAge = null)
        {
            _statePensionAge = statePensionAge;
            _privatePensionAge = privatePensionAge;
        }

        public DateTime StatePensionDate(DateTime dob, Sex sex)
        {
            return _statePensionAge;
        }

        public DateTime PrivatePensionDate(DateTime statePensionAge)
        {
            return _privatePensionAge ?? _statePensionAge.AddYears(-10);
        }
    }
}