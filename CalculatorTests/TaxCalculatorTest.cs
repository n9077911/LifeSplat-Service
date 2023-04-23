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
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncomeForTax(50_000, 0, 5000)).IncomeTax, Is.EqualTo(6484.2m));
            //only rental income where tax falls in 40% bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(0m, rentalIncome: new RentalIncomeForTax(100_000, 0, 10_000)).IncomeTax, Is.EqualTo(25_428.4m));

            
            //personal allowance is breached due to rental properties 
            Assert.That(new IncomeTaxCalculator().TaxFor(3_000m, rentalIncome: new RentalIncomeForTax(10_000, 0, 5_000)).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(3_000m, rentalIncome: new RentalIncomeForTax(11_000, 0, 0)).IncomeTax, Is.EqualTo(84.2));
            
            //rental puts you in 100k bracket
            Assert.That(new IncomeTaxCalculator().TaxFor(90_000, rentalIncome: new RentalIncomeForTax(20_000, 0, 10_000)).IncomeTax, Is.EqualTo(31_428.4m));

            //applies the property allowance when appropriate
            var taxReport = new IncomeTaxCalculator().TaxFor(10_000, rentalIncome: new RentalIncomeForTax(11_000, 400, 2_000));
            Assert.That(taxReport.IncomeTax, Is.EqualTo(1_484.2m));
            Assert.That(taxReport.AfterTaxIncomeFor(IncomeType.RentalIncome), Is.EqualTo(7_115.8m));

            //all income types
            var taxResult = new IncomeTaxCalculator().TaxFor(90_000, 10_000, 10_000, new RentalIncomeForTax(20_000, 0, 10_000));
            Assert.That(taxResult.IncomeTax, Is.EqualTo(42_703m));
            Assert.That(taxResult.NationalInsurance, Is.EqualTo(5_318.32));
            Assert.That(taxResult.TotalTax, Is.EqualTo(48_021.32));
        }

        [Test]
        public void CalculatesRentalTax_ConsideringPropertyAllowance()
        {
            var taxReport = new IncomeTaxCalculator().TaxFor(10_000, rentalIncome: new RentalIncomeForTax(10_000, 400, 2_000));
            Assert.That(taxReport.IncomeTax, Is.EqualTo(1284.2m));
            Assert.That(taxReport.AfterTaxIncomeFor(IncomeType.RentalIncome), Is.EqualTo(6315.8m));

            var taxReportAllowanceBroken = new IncomeTaxCalculator().TaxFor(10_000, rentalIncome: new RentalIncomeForTax(10_000, 800, 1_600)); //800+.2*1_600 is > 1_000
            Assert.That(taxReportAllowanceBroken.IncomeTax, Is.EqualTo(1004.2m));
            Assert.That(taxReportAllowanceBroken.AfterTaxIncomeFor(IncomeType.RentalIncome), Is.EqualTo(6595.8m));
        }

        [Test]
        public void CalculatesCorrectTax()
        {
            Assert.That(new IncomeTaxCalculator().TaxFor(0).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(0).NationalInsurance, Is.EqualTo(0));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(12_500).IncomeTax, Is.EqualTo(0));
            Assert.That(new IncomeTaxCalculator().TaxFor(12_500).NationalInsurance, Is.EqualTo(0));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(50_000).IncomeTax, Is.EqualTo(7_484.2));
            
            Assert.That(new IncomeTaxCalculator().TaxFor(100_000).IncomeTax, Is.EqualTo(27_428.4));

            Assert.That(new IncomeTaxCalculator().TaxFor(125_000).IncomeTax, Is.EqualTo(42_428.4));
            Assert.That(new IncomeTaxCalculator().TaxFor(150_000).IncomeTax, Is.EqualTo(53_703));
            
            var tax160000 = new IncomeTaxCalculator().TaxFor(160_000);
            Assert.That(tax160000.IncomeTax, Is.EqualTo(58_203));
            Assert.That(tax160000.NationalInsurance, Is.EqualTo(6_718.32m));
            Assert.That(tax160000.TotalTax, Is.EqualTo(64_921.32m));
        }

        [Test]
        public void IncomeTaxIsSeparatedPerIncomeType()
        {
            var result = new IncomeTaxCalculator().TaxFor(60_000, 60_000, 60_000);
            
            Assert.That(result.TotalTaxFor(IncomeType.Salary), Is.EqualTo(21_178.32m));
            Assert.That(result.TotalTaxFor(IncomeType.PrivatePension), Is.EqualTo(24_000));
            Assert.That(result.TotalTaxFor(IncomeType.StatePension), Is.EqualTo(26_743));
        }
        
        [Test]
        public void IncomeTaxFromThreeSourcesIsSameAsTaxFromOneSource()
        {
            var taxFromOneSource = new IncomeTaxCalculator().TaxFor(180_000).IncomeTax;
            var taxFromThreeSources = new IncomeTaxCalculator().TaxFor(60_000, 60_000, 60_000).IncomeTax;
            Assert.That(taxFromOneSource, Is.EqualTo(taxFromThreeSources));
        }
        
        [Test]
        public void NationalInsuranceIsNotPayableOnPensionIncome()
        {
            Assert.That(new IncomeTaxCalculator().TaxFor(60_000).NationalInsurance, Is.EqualTo(4_718.32m));
            Assert.That(new IncomeTaxCalculator().TaxFor(60_000, 60_000, 60_000).NationalInsurance, Is.EqualTo(4718.32m));
        }
    }
}
