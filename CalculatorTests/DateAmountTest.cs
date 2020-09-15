using System;
using NUnit.Framework;
using TaxCalculator;

namespace CalculatorTests
{
    public class DateAmountTest
    {
        [Test]
        public void CanDescribeATimePeriod()
        {
            Assert.That(new DateAmount(new DateTime(2020, 07, 25),  new DateTime(2022, 07, 25)).ToString(),
                Is.EqualTo("2 Years and 0 Months"));
            Assert.That(new DateAmount(new DateTime(2020, 07, 25),  new DateTime(2022, 12, 25)).ToString(),
                Is.EqualTo("2 Years and 5 Months"));
            Assert.That(new DateAmount(new DateTime(2020, 07, 25),  new DateTime(2022, 05, 25)).ToString(),
                Is.EqualTo("1 Year and 10 Months"));
            
            Assert.That(new DateAmount(new DateTime(2020, 07, 25),  new DateTime(2021, 08, 25)).ToString(),
                Is.EqualTo("1 Year and 1 Month"));
        }
    }
}