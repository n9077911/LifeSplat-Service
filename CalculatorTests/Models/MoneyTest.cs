using Calculator;
using NUnit.Framework;

namespace CalculatorTests.Models
{
    [TestFixture]
    public class MoneyTest
    {
        [Test]
        public void CanBeDefinedInThousands()
        {
            Assert.That(Money.Create("12k").Value, Is.EqualTo(12000m));
            Assert.That(Money.Create("12K").Value, Is.EqualTo(12000m));
            Assert.That(Money.Create("12000").Value, Is.EqualTo(12000m));
            Assert.That(Money.Create("1k*12").Value, Is.EqualTo(12000m));
            Assert.That(Money.Create("1000*12").Value, Is.EqualTo(12000m));
            Assert.That(Money.Create("1000x12").Value, Is.EqualTo(12000m));
            Assert.That(Money.Create("1000X12").Value, Is.EqualTo(12000m));
            Assert.That(Money.Create("12X1k").Value, Is.EqualTo(12000m));
        }
    }
}