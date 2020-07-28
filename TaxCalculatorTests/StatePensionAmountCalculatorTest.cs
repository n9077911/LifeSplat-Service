using System;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;

namespace TaxCalculatorTests
{
    [TestFixture]
    public class StatePensionAmountCalculatorTest
    {
        [Test]
        public void CalcPensionAge()
        {
            var calc = new StatePensionAmountCalculator();

            var amount = calc.Calculate(new PersonStatus {Dob = new DateTime(1981, 5, 30)}, new DateTime(2050, 1, 1));
            Assert.That(amount, Is.EqualTo(9110.4m));
            
            amount = calc.Calculate(new PersonStatus {Dob = new DateTime(1981, 5, 30)}, new DateTime(2020, 5, 30));
            Assert.That(amount, Is.EqualTo(4685.35m));
            
            //minimum 10 contributing years required
            amount = calc.Calculate(new PersonStatus {Dob = new DateTime(1981, 5, 30)}, new DateTime(2011, 5, 30));
            Assert.That(amount, Is.EqualTo(0m));
        }
    }
}