using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calculator
{
    public class BadInputException : Exception
    {
        public BadInputException(string message) : base(message)
        {
        }
    }


    public class Money
    {
        private Money(in decimal number)
        {
            Value = number;
        }

        public static Money Create(int input)
        {
            return new Money(input);
        }
        
        public static Money Create(decimal input)
        {
            return new Money(input);
        }

        public static Money Create(string input)
        {
            if(string.IsNullOrWhiteSpace(input))
                return new Money(0);
            
            var m = new Regex(@"^(?<number1>\d+)(?<thousands1>[kK])?([xX*](?<number2>\d+)(?<thousands2>[kK])?)?$").Match(input);
            if (m.Success)
            {
                var number = Convert.ToInt32(m.Groups["number1"].Value);
                if (m.Groups["thousands1"].Success)
                    number *= 1000;

                var number2 = 1;
                if (m.Groups["number2"].Success)
                    number2 = Convert.ToInt32(m.Groups["number2"].Value);
                if (m.Groups["thousands2"].Success)
                    number2 *= 1000;

                return new Money(number*number2);
            }

            throw new Exception($"Failed to parse as money {input}");
        }

        public decimal Value { get; }

        public static implicit operator decimal(Money m) => m.Value;
        public static implicit operator int(Money m) => (int)m.Value; //Loss of precision. Not deemed significant.
        public static implicit operator Money(int m) => Create(m);
        public static implicit operator Money(decimal m) => Create(m);

    }

    public static class MoneyExtensions
    {
        public static Money Sum(this IEnumerable<Money> monies)
        {
            decimal input = monies.Select(m => m.Value).Sum();
            return Money.Create(input);
        }
    }
}