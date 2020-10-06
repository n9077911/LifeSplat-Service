using System.Collections.Generic;
using NUnit.Framework;
using Calculator.TaxSystem;

namespace CalculatorTests
{
    public class TaxCalculatorTests
    {
        [Test]
        public void CalculatesRentalIncomeTax()
        {
            //only rental income where income is below tax band
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncomeForTax(12000, 0, 5000)).IncomeTax, Is.EqualTo(0));
            //only rental income where tax falls in 20% bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncomeForTax(50_000, 0, 5000)).IncomeTax, Is.EqualTo(6498.2m));
            //only rental income where tax falls in 40% bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncomeForTax(100_000, 0, 10_000)).IncomeTax, Is.EqualTo(25496.4m));

            
            //personal allowance is breached due to rental properties but 
            Assert.That(new IncomeTaxCalculator().TaxFor(3_000m, rentalIncome: new RentalIncomeForTax(10_000, 0, 5_000)).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(3_000m, rentalIncome: new RentalIncomeForTax(10_000, 0, 0)).IncomeTax, Is.EqualTo(98.2));
            
            //rental puts you in 100k bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(90_000, rentalIncome: new RentalIncomeForTax(20_000, 0, 10_000)).IncomeTax, Is.EqualTo(31_496.4m));

            //applies the property allowance when appropriate
            var taxReport = new IncomeTaxCalculator().TaxFor(10_000, rentalIncome: new RentalIncomeForTax(11_000, 400, 2_000));
            Assert.That(taxReport.IncomeTax, Is.EqualTo(1_218.2m));
            Assert.That(taxReport.AfterTaxIncomeFor(IncomeType.RentalIncome), Is.EqualTo(8981.8m));

            //all income types
            var taxResult = new IncomeTaxCalculator().TaxFor(90_000, 10_000, 10_000, new RentalIncomeForTax(20_000, 0, 10_000));
            Assert.That(taxResult.IncomeTax, Is.EqualTo(42_500m));
            Assert.That(taxResult.NationalInsurance, Is.EqualTo(5_660));
            Assert.That(taxResult.TotalTax, Is.EqualTo(48_160));
        }

        [Test]
        public void CalculatesRentalTax_ConsideringPropertyAllowance()
        {
            var taxReport = new IncomeTaxCalculator().TaxFor(10_000, rentalIncome: new RentalIncomeForTax(10_000, 400, 2_000));
            Assert.That(taxReport.IncomeTax, Is.EqualTo(1298.2m));
            Assert.That(taxReport.AfterTaxIncomeFor(IncomeType.RentalIncome), Is.EqualTo(6301.8m));

            var taxReportAllowanceBroken = new IncomeTaxCalculator().TaxFor(10_000, rentalIncome: new RentalIncomeForTax(10_000, 800, 1_600)); //800+.2*1_600 is > 1_000
            Assert.That(taxReportAllowanceBroken.IncomeTax, Is.EqualTo(1018.2m));
            Assert.That(taxReportAllowanceBroken.AfterTaxIncomeFor(IncomeType.RentalIncome), Is.EqualTo(6581.8m));
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
