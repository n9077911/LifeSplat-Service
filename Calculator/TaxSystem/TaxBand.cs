namespace Calculator.TaxSystem;

/// <summary>
/// Represents a single tax band 
/// </summary>
public class TaxBand
{
    public int UpperBound { get; }
    public int LowerBound { get; }
    public decimal Rate { get; }

    public TaxBand(int upperBound, int lowerBound, decimal rate)
    {
        UpperBound = upperBound;
        LowerBound = lowerBound;
        Rate = rate;
    }
}