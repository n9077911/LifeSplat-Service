using System;

namespace Calculator
{
    public class PensionContribution
    {
        private readonly Percent _percent;

        private PensionContribution(Percent percent)
        {
            _percent = percent;
        }

        public static PensionContribution Create(string pensionContribution)
        {
            if (string.IsNullOrWhiteSpace(pensionContribution))
                return Create(0);
            
            if (!Decimal.TryParse(pensionContribution, out var result))
                throw new BadInputException($"'{pensionContribution}' is not a valid Pension Contribution");
            
            return Create(result);
        }
        
        public static PensionContribution Create(decimal pensionContribution)
        {
            if(pensionContribution<0 ||pensionContribution>100)
                throw new BadInputException($"'{pensionContribution}' Pension Contribution must be between 0% and 100%");
                 
            return new PensionContribution(Percent.Create(pensionContribution));
        }
        
        public static implicit operator PensionContribution(int m) => Create(m);
        public static implicit operator PensionContribution(decimal m) => Create(m);

        public Money SubtractContribution(Money salary)
        {
            return salary - _percent.Of(salary);
        }
        
        public Money Amount(Money salary)
        {
            return _percent.Of(salary);
        }
    }
}