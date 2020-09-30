namespace Calculator.Input
{
    public interface IAssumptions
    {
        int EstimatedDeathAge { get; set; }
        decimal AnnualGrowthRate { get; set; }
        decimal MonthlyGrowthRate { get; }
        ///At pension age take 25% tax free
        bool Take25 { get; set; }
    }
}