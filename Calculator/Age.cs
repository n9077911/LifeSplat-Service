using System;
using CSharpFunctionalExtensions;

namespace Calculator
{
    public class Age
    {
        private int _age = 0;
        
        private Age(in int number)
        {
            _age = number;
        }

        public static Age Create(int input)
        {
            return new Age(input);
        }

        public static Maybe<Age> Create(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Maybe<Age>.None;

            if (Int32.TryParse(input, out var asInt))
                return new Age(asInt);

            throw new BadInputException($"{input} does not specify an Age");
        }

        public DateTime DateFrom(in DateTime dob)  
        {
            return dob.AddYears(_age);
        }
        
        public static implicit operator Age(int a) => new Age(a);
    }
}