using System;

namespace TaxCalculator
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

        public DateTime StatePensionDate(DateTime personStatusDob)
        {
            return _statePensionAge;
        }

        public DateTime PrivatePensionDate()
        {
            return _privatePensionAge ?? _statePensionAge;
        }
    }
}