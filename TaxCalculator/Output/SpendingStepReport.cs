using System;

namespace TaxCalculator.Output
{
    public class SpendingStepReport
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int Spending { get; }

        public SpendingStepReport(DateTime startDate, DateTime endDate, int spending)
        {
            StartDate = startDate;
            EndDate = endDate;
            Spending = spending;
        }
    }
}