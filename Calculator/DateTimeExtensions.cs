using System;

namespace Calculator
{
    public static class DateTimeExtensions
    {
        public static int WholeYearsUntil(this DateTime date, DateTime until)
        {
            if (date.Month < until.Month || (date.Month == until.Month && date.Day <= until.Day))
                return until.Year - date.Year;
            return until.Year - date.Year - 1;
        }
    }
}