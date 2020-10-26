using System;

namespace Calculator
{
    public class Percent
    {
        private readonly decimal _value;

        private Percent(decimal value)
        {
            _value = value;
        }

        public static Percent Create(string value)
        {
            if(!Decimal.TryParse(value, out var x))
            {
                throw new BadInputException($"'{value}' is not a valid Percentage input");
            }
            
            return string.IsNullOrEmpty(value) ? new Percent(0) : new Percent(Convert.ToDecimal(value));
        }
        
        public static Percent Create(decimal value)
        {
            return new Percent(value);
        }

        public decimal Of(decimal amount)
        {
            return amount * (_value / 100);
        }
    }
}