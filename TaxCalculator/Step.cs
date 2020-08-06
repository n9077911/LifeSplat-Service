using System;

namespace TaxCalculator
{
    public class Step
    {
        public DateTime Date { get; set; }
        public decimal Savings { get; set; }
        public decimal StatePension { get; set; }
        public decimal AfterTaxSalary { get; set; }
        public decimal Growth { get; set; }
        public decimal PrivatePensionGrowth { get; set; }
        public decimal PrivatePensionAmount { get; set; }
    }
}