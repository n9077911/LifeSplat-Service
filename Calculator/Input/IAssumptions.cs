namespace Calculator.Input
{
    public interface IAssumptions
    {
        int EstimatedDeathAge { get; }
        decimal AnnualGrowthRate { get; }
        decimal MonthlyGrowthRate { get; }
    }
}