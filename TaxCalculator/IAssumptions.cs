namespace TaxCalculator
{
    public interface IAssumptions
    {
        int EstimatedDeath { get; }
        decimal GrowthRate { get; }
    }
}