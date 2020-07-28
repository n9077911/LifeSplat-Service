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
                {Salary = 100_000, Spending = 40_000, Dob = new DateTime(1981, 05, 30), Sex = Sex.Female});

            //TODO: delete, used for debugging, once steps calculation stabilises this should be converted to an assert.
            var steps = report.Steps.Where((step, i) => i % 12 == 0).ToList();
            for (int i = 0; i < steps.Count; i++)
            {
                Console.WriteLine($"{i} : {steps[i].Cash}");
            }
        }    
    }
}