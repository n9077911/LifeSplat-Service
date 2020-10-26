using System;

namespace Calculator.Output
{
    public class SpendingStepReport
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public Money Spending { get; }

        public SpendingStepReport(DateTime startDate, DateTime endDate, Money spending)
        {
            StartDate = startDate;
            EndDate = endDate;
            Spending = spending;
        }
    }
}