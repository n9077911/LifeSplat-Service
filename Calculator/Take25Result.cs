namespace Calculator
{
    public class Take25Result
    {
        public decimal PensionPotBeforeTake25 { get; }
        public decimal NewPensionPot { get; }
        public decimal TaxFreeAmount { get; }
        public decimal LtaCharge { get; }

        public Take25Result(in decimal originalPensionPot, in decimal newPensionPot, in decimal taxFreeAmount, in decimal ltaCharge)
        {
            PensionPotBeforeTake25 = originalPensionPot;
            NewPensionPot = newPensionPot;
            TaxFreeAmount = taxFreeAmount;
            LtaCharge = ltaCharge;
        }
    }
}