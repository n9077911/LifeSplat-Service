using System;
using NUnit.Framework;
using TaxCalculator;

namespace TaxCalculatorTests
{
    public class TaxCalculatorTests
    {
        [Test]
        public void ShouldPayNoTax()
        {
            Assert.That(new IncomeTaxCalculator().TaxFor(0).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(0).NationalInsurance, Is.EqualTo(0));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(12_500).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(12_500).NationalInsurance, Is.EqualTo(360));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(50_000).IncomeTax, Is.EqualTo(7_498.2));
            Assert.That(new IncomeTaxCalculator().TaxFor(100_000).IncomeTax, Is.EqualTo(27_496.4));
            Assert.That(new IncomeTaxCalculator().TaxFor(125_000).IncomeTax, Is.EqualTo(42_496.4));
            Assert.That(new IncomeTaxCalculator().TaxFor(150_000).IncomeTax, Is.EqualTo(52_500));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(160_000).IncomeTax, Is.EqualTo(57_000));
            Assert.That(new IncomeTaxCalculator().TaxFor(160_000).NationalInsurance, Is.EqualTo(7_060));
        }
    }

}
