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
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, new PensionAgeCalc(), new StatePensionAmountCalculator(_fixedDateProvider));
            var report = calc.ReportFor(new PersonStatus
                {
                    Spending = 40_000,
                    Salary = 100_000, 
                    Dob = new DateTime(1981, 05, 30), 
                    Sex = Sex.Female,
                    ExistingSavings = 50_000,
                    ExistingPrivatePension = 100_000,
                    EmployerContribution = 0.03m,
                    EmployeeContribution = 0.05m,
                });

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2034, 01, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(52));
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(397_123));
            Assert.That(report.PrimaryPerson.PrivatePensionSafeWithdrawal, Is.EqualTo(15_885));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("14 Years and 0 Months"));
        }    
    }
}