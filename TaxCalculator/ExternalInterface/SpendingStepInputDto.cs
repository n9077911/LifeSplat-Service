using System;

namespace TaxCalculator.ExternalInterface
{
    public class SpendingStepInput
    {
        public DateTime Date { get; }
        public int NewAmount { get; }

        public SpendingStepInput(DateTime date, int newAmount)
        {
            Date = date;
            NewAmount = newAmount;
        }
    }
}