namespace Calculator.Input
{
    public class CashSavingsSpec
    {
        private readonly decimal _amount;
        private readonly decimal _months;
        private readonly bool _isMonths;

        public CashSavingsSpec(string amount)
        {
            if (amount.EndsWith('m') || amount.EndsWith('M'))
            {
                _months = decimal.Parse(amount.Substring(0, amount.Length - 1));
                _isMonths = true;
            }
            else if (amount.EndsWith('k') || amount.EndsWith('K'))
                _amount = decimal.Parse(amount.Substring(0, amount.Length - 1))*1000;
            else
                _amount = decimal.Parse(amount == "" ? "0" : amount);
        }

        public decimal RequiredSavings(decimal spending)
        {
            if (_isMonths)
                return spending * _months;
            return _amount;
        }

        public CashSavingsSpec SplitInTwo()
        {
            return new CashSavingsSpec(_isMonths?(_months/2)+"m" :(_amount/2).ToString());
        }
    }
}