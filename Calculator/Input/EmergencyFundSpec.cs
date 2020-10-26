namespace Calculator.Input
{
    public class EmergencyFundSpec
    {
        private readonly Money _amount;
        private readonly decimal _months;
        private readonly bool _isMonths;

        public EmergencyFundSpec(string amount)
        {
            if (amount == null)
            {
                _amount = 0;
            }
            else if (amount.EndsWith('m') || amount.EndsWith('M'))
            {
                _months = decimal.Parse(amount.Substring(0, amount.Length - 1));
                _isMonths = true;
            }
            else
            {
                _amount = Money.Create(amount);
                _isMonths = false;
            }
            
        }

        public decimal RequiredEmergencyFund(decimal spending)
        {
            if (_isMonths)
                return spending * _months;
            return _amount;
        }

        public EmergencyFundSpec SplitInTwo()
        {
            return new EmergencyFundSpec(_isMonths?(_months/2)+"m" :(_amount/2).ToString());
        }
    }
}