using System;

namespace TaxCalculator.ExternalInterface
{
    public class PersonStatus
    {
        public int Salary { get; set; }
        public int Spending { get; set; }
        public DateTime Dob { get; set; }
        public int Amount { get; set; }
        public Sex Sex { get; set; }
    }
}