using System;
using System.Linq;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;

namespace TaxCalculatorTests
{
    [TestFixture]
    public class RetirementIncrementalApproachCalculatorTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly SafeWithDrawlNoInflationAssumptions _assumptions = new SafeWithDrawlNoInflationAssumptions();
        private readonly StubPensionAgeCalc _pensionAgeCalc = new StubPensionAgeCalc(new DateTime(2049, 05, 30));

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_ThisYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 100, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2020, 02, 1)));
            Assert.That(report.RetirementAge, Is.EqualTo(38));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("0 Years and 1 Months"));
        }

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_NextYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 2_500, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2022, 03, 1)));
            Assert.That(report.RetirementAge, Is.EqualTo(40));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("2 Years and 2 Months"));

            var steps = report.Steps.Where((step, i) => i % 12 == 0).ToList();
            for (int i = 0; i < steps.Count; i++)
            {
                Console.WriteLine($"{i} : {steps[i].Cash}");
            }

        }
    }
}