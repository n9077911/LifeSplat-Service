using System;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;

namespace TaxCalculatorTests
{
    [TestFixture]
    public class RetirementCalculatorTest
    {
        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_ThisYear()
        {
            var calc = new RetirementCalculator(new FixedDateProvider(new DateTime(2020, 1, 1)));
            var report = calc.ReportFor(new PersonStatus {Salary = 30_000, Spending = 100, Dob = new DateTime(1981, 05, 30)});
            
            Assert.That(report.TargetSavings, Is.EqualTo(2500));
            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2020, 03, 1)));
            Assert.That(report.RetirementAge, Is.EqualTo(38));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("0 Years and 2 Months"));
        }
        
        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_NextYear()
        {
            var calc = new RetirementCalculator(new FixedDateProvider(new DateTime(2020, 1, 1)));
            var report = calc.ReportFor(new PersonStatus {Salary = 30_000, Spending = 1_100, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.TargetSavings, Is.EqualTo(27_500));
            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2021, 04, 1)));
            Assert.That(report.RetirementAge, Is.EqualTo(39));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("1 Years and 3 Months"));
        }
    }
}
