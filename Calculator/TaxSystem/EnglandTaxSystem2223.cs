using System;
using System.Collections.Generic;

namespace Calculator.TaxSystem;

public class EnglandTaxSystem2223 : ITaxSystem
{
    public int LowerEarningsLimit => 6_396;
        
    public IEnumerable<TaxBand> NationalInsuranceBands => new[] {new TaxBand(Int32.MaxValue, 50_284, .02m),  new TaxBand(50_284, 12_584, .12m) };
        
    public int PersonalAllowance => 12_579;

    public int BasicRateBand => 37_700;
    public decimal BasicRate => .20m;

    public int HigherRateUpperBound => 125_140;
    public decimal HigherRate => .40m;

    public decimal AdditionalRate => .45m;
        
    public int PersonalAllowanceTaperPoint => 100_000;
}