using System;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;
using TaxCalculatorTests.Stubs;

namespace TaxCalculatorTests
{
    [TestFixture]
    public class RetirementIncrementalApproachCalculatorTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly SafeWithdrawalNoInflationAssumptions _assumptions = new SafeWithdrawalNoInflationAssumptions();
        private readonly StubPensionAgeCalc _pensionAgeCalc = new StubPensionAgeCalc(new DateTime(2049, 05, 30));
        private readonly FixedStatePensionAmountCalculator _fixedStatePensionAmountCalculator = new FixedStatePensionAmountCalculator(9110.4m);

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_ThisYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 100, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2020, 02, 1)));
            Assert.That(report.RetirementAge, Is.EqualTo(38));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("0 Years and 1 Months"));
        }

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_NextYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 2_500, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2021, 12, 01)));
            Assert.That(report.RetirementAge, Is.EqualTo(40));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("1 Years and 11 Months"));
        }

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_CloseToStatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2048, 08, 01)));
            Assert.That(report.RetirementAge, Is.EqualTo(67));
            Assert.That(report.StateRetirementAge, Is.EqualTo(68));
            Assert.That(report.StateRetirementDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("28 Years and 7 Months"));
        }
        
        [Test]
        public void ConsidersExistingSavings()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {ExistingSavings = 50_000, Salary = 30_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2042, 11, 01)));
            Assert.That(report.RetirementAge, Is.EqualTo(61));
            Assert.That(report.StateRetirementAge, Is.EqualTo(68));
            Assert.That(report.StateRetirementDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("22 Years and 10 Months"));
        }
    }
}