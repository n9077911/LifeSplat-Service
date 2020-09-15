namespace Calculator.StatePensionCalculator
{
    public readonly struct StatePensionResult
    {
        public int ContributingYears { get; }
        public decimal Amount { get; }

        public StatePensionResult(in int contributingYears, in decimal amount)
        {
            ContributingYears = contributingYears;
            Amount = amount;
        }
    }
}