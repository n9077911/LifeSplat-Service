using System;
using NUnit.Framework;
using TaxCalculator;

namespace CoreTests
{
    [TestFixture]
    public class RetirementCalculatorTest
    {
        /*
         * As an employee
         * I want to know when I can retire
         * So that I can plan for the future
         *
         * As an employee
         * I want to know whether to put savings in my pension or ISA
         * To retire earliest
         */
        [Test]
        public void KnowsWhenYouCanRetire()
        {
            var calc = new RetirementCalculator();
            IRetirementReport report = calc.ReportFor(new PersonStatus {Salary = 30_000, Spending = 100, Dob = new DateTime(1981, 05, 30)});
            
            Assert.That(report.TargetSavings, Is.EqualTo(2500));
            // Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(30, 05, 1981)));
        }

    }

}
