using System;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;

namespace TaxCalculatorTests
{
    [TestFixture]
    public class StatePensionAmountCalculatorTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));

        [Test]
        public void CalcsStatePensionAmount_BasedOnAssumedContributions()
        {
            var calc = new StatePensionAmountCalculator(_fixedDateProvider, new TwentyTwentyTaxSystem());

            var amount = calc.Calculate(new PersonStatus {Dob = new DateTime(1981, 5, 30), Salary = 10_000}, new DateTime(2050, 1, 1)).Item2;
            Assert.That(amount, Is.EqualTo(9110.4m));
            
            amount = calc.Calculate(new PersonStatus {Dob = new DateTime(1981, 5, 30), Salary = 10_000}, new DateTime(2020, 5, 30)).Item2;
            Assert.That(amount, Is.EqualTo(4685.35m));
            
            //minimum 10 contributing years required
            amount = calc.Calculate(new PersonStatus {Dob = new DateTime(1981, 5, 30), Salary = 10_000}, new DateTime(2011, 5, 30)).Item2;
            Assert.That(amount, Is.EqualTo(0m));
        }

        [Test]
        public void CalcsStatePensionAmount_BasedOnGivenExistingContributions()
        {
            var calc = new StatePensionAmountCalculator(_fixedDateProvider, new TwentyTwentyTaxSystem());

            var amount = calc.Calculate(new PersonStatus {NiContributingYears = 10, Salary = 10_000}, new DateTime(2050, 1, 1)).Item2;
            Assert.That(amount, Is.EqualTo(9110.4m));
            
            amount = calc.Calculate(new PersonStatus {NiContributingYears = 8, Salary = 10_000}, new DateTime(2030, 5, 30)).Item2;
            Assert.That(amount, Is.EqualTo(4685.35m));
            
            //minimum 10 contributing years required
            amount = calc.Calculate(new PersonStatus {NiContributingYears = 9, Salary = 10_000}, new DateTime(2011, 5, 30)).Item2;
            Assert.That(amount, Is.EqualTo(0m));
        }
        
        [Test]
        public void CalcsStatePensionAmount_WhenPersonEarnsLessThanTheMinimum()
        {
            var calc = new StatePensionAmountCalculator(_fixedDateProvider, new TwentyTwentyTaxSystem());

            var amount = calc.Calculate(new PersonStatus {NiContributingYears = 10, Salary = 5_000}, new DateTime(2050, 1, 1)).Item2;
            Assert.That(amount, Is.EqualTo(2602.97));
            
            amount = calc.Calculate(new PersonStatus {NiContributingYears = 9, Salary = 5_000}, new DateTime(2050, 1, 1)).Item2;
            Assert.That(amount, Is.EqualTo(0));
            
            amount = calc.Calculate(new PersonStatus {Dob = new DateTime(1981, 5, 30), Salary = 5_000}, new DateTime(2050, 1, 1)).Item2;
            Assert.That(amount, Is.EqualTo(0));
        }
    }
}