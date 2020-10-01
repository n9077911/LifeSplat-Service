using System.Collections.Generic;
using NUnit.Framework;
using Calculator;
using Calculator.TaxSystem;

namespace CalculatorTests
{
    public class TaxCalculatorTests
    {
        [Test]
        public void CalculatesRentalIncomeTax()
        {
            //only rental income where income is below tax band
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncome(12000, 5000)).IncomeTax, Is.EqualTo(0));
            //only rental income where tax falls in 20% bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncome(50_000, 5000)).IncomeTax, Is.EqualTo(6498.2m));
            //only rental income where tax falls in 40% bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncome(100_000, 10_000)).IncomeTax, Is.EqualTo(25496.4m));

            
            //personal allowance is breached due to rental properties but 
            Assert.That(new IncomeTaxCalculator().TaxFor(3_000m, rentalIncome: new RentalIncome(10_000, 5_000)).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(3_000m, rentalIncome: new RentalIncome(10_000, 0)).IncomeTax, Is.EqualTo(98.2));
            
            //rental puts you in 100k bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(90_000, rentalIncome: new RentalIncome(20_000, 10_000)).IncomeTax, Is.EqualTo(31_496.4m));

            //all income types
            Assert.That(new IncomeTaxCalculator().TaxFor(90_000, 10_000, 10_000, new RentalIncome(20_000, 10_000)).IncomeTax, Is.EqualTo(42500m));
        }
        
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
