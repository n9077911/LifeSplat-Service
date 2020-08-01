using System;
using System.Linq;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;

namespace TaxCalculatorTests.IntegrationTests
{
    [TestFixture]
    public class IntegrationRetirementCalculatorTest
    {
        private readonly SafeWithdrawalNoInflationAssumptions _assumptions = new SafeWithdrawalNoInflationAssumptions();
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));

        [Test]
        public void HighEarning_FortyYearOld_Woman()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, new PensionAgeCalc(), new StatePensionAmountCalculator());
            var report = calc.ReportFor(new PersonStatus
                {
                    Spending = 40_000,
                    Salary = 100_000, 
                    Dob = new DateTime(1981, 05, 30), 
                    Sex = Sex.Female,
                    ExistingSavings = 50_000,
                });

            Assert.That(report.RetirementDate, Is.EqualTo(new DateTime(2037, 02, 01)));
            Assert.That(report.RetirementAge, Is.EqualTo(55));
            Assert.That(report.StateRetirementAge, Is.EqualTo(68));
            Assert.That(report.StateRetirementDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("17 Years and 1 Months"));
            
            //TODO: delete, used for debugging, once steps calculation stabilises this should be converted to an assert.
            var steps = report.Steps.Where((step, i) => i % 12 == 0).ToList();
            for (int i = 0; i < steps.Count; i++)
            {
                Console.WriteLine($"{i} : {steps[i].Savings}");
            }
        }    
    }
}