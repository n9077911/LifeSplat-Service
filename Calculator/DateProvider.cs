using System;

namespace TaxCalculator
{
    public interface IDateProvider
    {
        DateTime Now();
    }

    public class DateProvider : IDateProvider
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }


    public class FixedDateProvider : IDateProvider
    {
        private readonly DateTime _dateTime;

        public FixedDateProvider(DateTime dateTime)
        {
            _dateTime = dateTime;
        }

        public DateTime Now()
        {
            return _dateTime;
        }
    }
}