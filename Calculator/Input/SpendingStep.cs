using System;

namespace Calculator.Input
{
    public class SpendingStep
    {
        public DateTime Date { get; }
        public Money NewAmount { get; }

        public SpendingStep(DateTime date, Money newAmount)
        {
            Date = date;
            NewAmount = newAmount;
        }
    }
    
    public class SalaryStep
    {
        public DateTime Date { get; }
        public Money NewAmount { get; }

        public SalaryStep(DateTime date, Money newAmount)
        {
            Date = date;
            NewAmount = newAmount;
        }
    }
}