namespace Calculator
{
    /// <summary>
    /// The pots of money that a person has e.g. Emergency Fund and Investments
    /// Understands income or spending should be taken from those pots.
    /// </summary>
    public class MoneyPots
    {
        private readonly decimal _requiredEmergencyFund;

        public MoneyPots(decimal requiredEmergencyFund)
        {
            _requiredEmergencyFund = requiredEmergencyFund;
        }

        public MoneyPots(in decimal investments, in decimal emergencyFund, in decimal requiredEmergencyFund)
        {
            _requiredEmergencyFund = requiredEmergencyFund;
            EmergencyFund = emergencyFund;
            Investments = investments;
        }

        public decimal EmergencyFund { get; set; }
        public decimal Investments { get; set; }

        public void AssignSpending(decimal spending)
        {
            Investments -= spending;
            Rebalance();
        }

        public void AssignIncome(in decimal newIncome)
        {
            Investments += newIncome;
            Rebalance();
        }

        public void Rebalance()
        {
            if (EmergencyFund < _requiredEmergencyFund)
            {
                var newlyRequiredAmount = _requiredEmergencyFund - EmergencyFund;
                if (Investments > newlyRequiredAmount)
                {
                    EmergencyFund = _requiredEmergencyFund;
                    Investments -= newlyRequiredAmount;
                }
                else
                {
                    EmergencyFund += Investments;
                    Investments = 0;
                }
            }
            else
            {
                Investments += EmergencyFund - _requiredEmergencyFund;
                EmergencyFund = _requiredEmergencyFund;
            }
        }

        public static MoneyPots From(MoneyPots pots, decimal requiredEmergencyFund)
        {
            return new MoneyPots(pots.Investments, pots.EmergencyFund, requiredEmergencyFund);
        }
    }
}