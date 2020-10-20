using System;
using System.Collections.Generic;
using System.Linq;
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

    [TestFixture]
    public class ChileBenefitCalcTest
    {
        [Test]
        public void CalculatesHowMuchChildBenefitAFamilyReceive()
        {
            var now = new DateTime(2020, 1, 1);
            
            //1 child under 18
            var amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1)}, 50_000);
            Assert.That(amount, Is.EqualTo(91).Within(1));
            
            //2 kids, 1 under 18
            amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1), new DateTime(2002, 1, 1)}, 50_000);
            Assert.That(amount, Is.EqualTo(91).Within(1));
            
            //3 kids under 18
            amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1), new DateTime(2018, 11, 30), new DateTime(2018, 11, 30)}, 50_000);
            Assert.That(amount, Is.EqualTo(212).Within(1));
            
            //3 kids under 18 50% clawback via 55k salary
            amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1), new DateTime(2018, 11, 30), new DateTime(2018, 11, 30)}, 55_000);
            Assert.That(amount, Is.EqualTo(106).Within(1));
            
            //3 kids under 75% clawback 
            amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1), new DateTime(2018, 11, 30), new DateTime(2018, 11, 30)}, 57_500);
            Assert.That(amount, Is.EqualTo(53).Within(1));
            
            //3 kids under 100% clawback 
            amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1), new DateTime(2018, 11, 30), new DateTime(2018, 11, 30)}, 60_000);
            Assert.That(amount, Is.EqualTo(0).Within(1));
            
            //3 kids under 110% clawback 
            amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1), new DateTime(2018, 11, 30), new DateTime(2018, 11, 30)}, 65_000);
            Assert.That(amount, Is.EqualTo(0).Within(1));
            
            //3 kids under 18 - claw back % is rounded down
            amount = ChildBenefitCalc.Amount(now, new List<DateTime> {new DateTime(2015, 1, 1), new DateTime(2018, 11, 30), new DateTime(2018, 11, 30)}, 55_099);
            Assert.That(amount, Is.EqualTo(106).Within(1));
        }
    }
}