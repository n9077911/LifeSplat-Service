using System;
using TaxCalculator;

namespace TaxCalculatorTests.Stubs
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

        public DateTime PrivatePensionDate(DateTime dob, Sex sex)
        {
            return _privatePensionAge ?? _statePensionAge;
        }
    }
}