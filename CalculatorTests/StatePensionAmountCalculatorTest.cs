using System;
using NUnit.Framework;
using Calculator;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;

namespace CalculatorTests
{
    [TestFixture]
    public class StatePensionAmountCalculatorTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));

        [Test]
        public void CalcsStatePensionAmount_BasedOnAssumedContributions()
        {
            var calc = new StatePensionAmountCalculator(_fixedDateProvider, new TwentyTwentyTaxSystem());

            var amount = calc.Calculate(new Person {Dob = new DateTime(1981, 5, 30), Salary = 10_000}, new DateTime(2050, 1, 1)).Amount;
            Assert.That(amount, Is.EqualTo(9110.4m));
            
            amount = calc.Calculate(new Person {Dob = new DateTime(1981, 5, 30), Salary = 10_000}, new DateTime(2020, 5, 30)).Amount;
            Assert.That(amount, Is.EqualTo(4685.35m));
            
            //minimum 10 contributing years required
            amount = calc.Calculate(new Person {Dob = new DateTime(1981, 5, 30), Salary = 10_000}, new DateTime(2011, 5, 30)).Amount;
            Assert.That(amount, Is.EqualTo(0m));
        }
        
        [Test]
        public void CalcsStatePensionAmount_BasedOnGivenExistingContributions()
        {
            var calc = new StatePensionAmountCalculator(_fixedDateProvider, new TwentyTwentyTaxSystem());

            var amount = calc.Calculate(new Person {NiContributingYears = 10, Salary = 10_000}, new DateTime(2050, 1, 1)).Amount;
            Assert.That(amount, Is.EqualTo(9110.4m));
            
            amount = calc.Calculate(new Person {NiContributingYears = 8, Salary = 10_000}, new DateTime(2030, 5, 30)).Amount;
            Assert.That(amount, Is.EqualTo(4685.35m));
            
            //minimum 10 contributing years required
            amount = calc.Calculate(new Person {NiContributingYears = 9, Salary = 10_000}, new DateTime(2011, 5, 30)).Amount;
            Assert.That(amount, Is.EqualTo(0m));
        }
        
        [Test]
        public void CalcsStatePensionAmount_WhenPersonEarnsLessThanTheMinimum()
        {
            var calc = new StatePensionAmountCalculator(_fixedDateProvider, new TwentyTwentyTaxSystem());

             //years given has an impact - salary has impact - no children
             var amount = calc.Calculate(new Person {NiContributingYears = 10, Salary = 5_000}, new DateTime(2050, 1, 1)).Amount;
             Assert.That(amount, Is.EqualTo(2602.97));
            
             //years given have no impact as < 10 - salary has no impact - children have no impact
             amount = calc.Calculate(new Person {NiContributingYears = 9, Salary = 5_000}, new DateTime(2050, 1, 1)).Amount;
             Assert.That(amount, Is.EqualTo(0));
            
             //no years given - salary has no impact - children have no impact
             amount = calc.Calculate(new Person {Dob = new DateTime(1981, 5, 30), Salary = 5_000}, new DateTime(2050, 1, 1)).Amount;
             Assert.That(amount, Is.EqualTo(0));
            
             //no years given - salary has no impact - child has impact
             var result = calc.Calculate(new Person {Dob = new DateTime(1981, 5, 30), Salary = 5_000, Children = {new DateTime(2015, 1, 1)}}, new DateTime(2050, 1, 1));
             Assert.That(result.Amount, Is.EqualTo(3123.57));
             Assert.That(result.ContributingYears, Is.EqualTo(12));
            
            //years given would have no impact due to < 10 - salary has no impact - children have impact and bring total above 10
            result = calc.Calculate(new Person {NiContributingYears = 9, Salary = 5_000, Children = {new DateTime(2015, 1, 1), new DateTime(2020, 1, 1)}},
                new DateTime(2050, 1, 1));
            Assert.That(result.Amount, Is.EqualTo(4164.75));
            Assert.That(result.ContributingYears, Is.EqualTo(16)); //9 from existing and 7 from eldest child. 
        }
    }
}