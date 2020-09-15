using System;
using NUnit.Framework;
using Calculator;

namespace CalculatorTests
{
    public class AgeCalcTest
    {
        [Test]
        public void CanCalculateAgeInYears()
        {
            Assert.That(AgeCalc.Age(new DateTime(2019, 05, 30), new DateTime(2020, 5, 30)), Is.EqualTo(1));
            Assert.That(AgeCalc.Age(new DateTime(2019, 05, 30), new DateTime(2020, 5, 31)), Is.EqualTo(1));
            Assert.That(AgeCalc.Age(new DateTime(2019, 05, 30), new DateTime(2020, 5, 29)), Is.EqualTo(0));
            Assert.That(AgeCalc.Age(new DateTime(2019, 05, 30), new DateTime(2020, 1, 31)), Is.EqualTo(0));
            Assert.That(AgeCalc.Age(new DateTime(2019, 05, 30), new DateTime(2020, 6, 1)), Is.EqualTo(1));
        }
    }
}