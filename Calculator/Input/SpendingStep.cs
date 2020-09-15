using System;

namespace Calculator.Input
{
    public class SpendingStep
    {
        public DateTime Date { get; }
        public int NewAmount { get; }

        public SpendingStep(DateTime date, int newAmount)
        {
            Date = date;
            NewAmount = newAmount;
        }
    }
}