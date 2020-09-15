namespace TaxCalculator.TaxSystem
{
    public class TwentyTwentyTaxSystem : ITaxSystem
    {
        /// <summary>
        /// Lower earnings limit - defines how much someone must earn to receive a NI stamp towards their pension
        /// </summary>
        public int LowerEarningsLimit => 6136;
    }
}