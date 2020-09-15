using NUnit.Framework;
using Calculator;
using Calculator.TaxSystem;

namespace CalculatorTests
{
    public class TaxCalculatorTests
    {
        [Test]
        public void CalculatesCorrectTax()
        {
            Assert.That(new IncomeTaxCalculator().TaxFor(0).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(0).NationalInsurance, Is.EqualTo(0));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(12_500).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(12_500).NationalInsurance, Is.EqualTo(360));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(50_000).IncomeTax, Is.EqualTo(7_498.2));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(100_000).IncomeTax, Is.EqualTo(27_496.4));

            Assert.That(new IncomeTaxCalculator().TaxFor(125_000).IncomeTax, Is.EqualTo(42_496.4));
            Assert.That(new IncomeTaxCalculator().TaxFor(150_000).IncomeTax, Is.EqualTo(52_500));
            
            var tax160000 = new IncomeTaxCalculator().TaxFor(160_000);
            Assert.That(tax160000.IncomeTax, Is.EqualTo(57_000));
            Assert.That(tax160000.NationalInsurance, Is.EqualTo(7_060));
            Assert.That(tax160000.TotalTax, Is.EqualTo(64_060));
        }

        [Test]
        public void IncomeTaxIsSeparatedPerIncomeType()
        {
            var result = new IncomeTaxCalculator().TaxFor(60_000, 60_000, 60_000);
            
            Assert.That(result.TotalTaxFor(IncomeType.Salary), Is.EqualTo(21_560));
            Assert.That(result.TotalTaxFor(IncomeType.PrivatePension), Is.EqualTo(24_000));
            Assert.That(result.TotalTaxFor(IncomeType.StatePension), Is.EqualTo(25_500));
        }
        
        [Test]
        public void IncomeTaxFromThreeSourcesIsSameAsTaxFromOneSource()
        {
            Assert.That(new IncomeTaxCalculator().TaxFor(180_000).IncomeTax, Is.EqualTo(66_000));
            Assert.That(new IncomeTaxCalculator().TaxFor(60_000, 60_000, 60_000).IncomeTax, Is.EqualTo(66_000));
        }
        
        [Test]
        public void NationalInsuranceIsNotPayableOnPensionIncome()
        {
            Assert.That(new IncomeTaxCalculator().TaxFor(60_000).NationalInsurance, Is.EqualTo(5_060));
            Assert.That(new IncomeTaxCalculator().TaxFor(60_000, 60_000, 60_000).NationalInsurance, Is.EqualTo(5_060));
        }
    }
}
