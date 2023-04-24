namespace Calculator.Input
{
    public interface IAssumptions
    {
        int EstimatedDeathAge { get; }
        decimal AnnualGrowthRate { get; }
        decimal MonthlyGrowthRate { get; }
        ///At pension age take 25% tax free
        bool Take25 { get; }
        int LifeTimeAllowance { get; }
    }
}