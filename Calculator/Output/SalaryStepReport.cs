using System;

namespace Calculator.Output
{
    public class SalaryStepReport
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public Money Salary { get; }

        public SalaryStepReport(DateTime startDate, DateTime endDate, Money salary)
        {
            StartDate = startDate;
            EndDate = endDate;
            Salary = salary;
        }
    }
}