namespace Calculator
{
    public class Take25Result
    {
        public decimal NewPensionPot { get; }
        public decimal TaxFreeAmount { get; }

        public Take25Result(in decimal newPensionPot, in decimal taxFreeAmount)
        {
            NewPensionPot = newPensionPot;
            TaxFreeAmount = taxFreeAmount;
        }
    }
}