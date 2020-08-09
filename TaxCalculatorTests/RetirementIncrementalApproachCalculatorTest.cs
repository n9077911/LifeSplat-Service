using System;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;
using TaxCalculatorTests.Stubs;

namespace TaxCalculatorTests
{
    [TestFixture]
    public class RetirementIncrementalApproachCalculatorTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly SafeWithdrawalNoInflationAssumptions _assumptions = new SafeWithdrawalNoInflationAssumptions();
        private readonly StubPensionAgeCalc _pensionAgeCalc = new StubPensionAgeCalc(new DateTime(2049, 05, 30));

        private readonly FixedStatePensionAmountCalculator _fixedStatePensionAmountCalculator =
            new FixedStatePensionAmountCalculator(9110.4m);

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_ThisYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 100, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2020, 02, 1)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(38));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("0 Years and 1 Month"));
        }

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_NextYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 2_500, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2021, 12, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(40));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("1 Year and 11 Months"));
        }

        [Test]
        public void KnowsWhenASalariedWorkerCanRetire_CloseToStatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {Salary = 30_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2048, 08, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(67));
            Assert.That(report.StateRetirementAge, Is.EqualTo(68));
            Assert.That(report.StateRetirementDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("28 Years and 7 Months"));
        }

        [Test]
        public void ConsidersExistingSavings()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
                {ExistingSavings = 50_000, Salary = 30_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30)});

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2042, 12, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(61));
            Assert.That(report.StateRetirementAge, Is.EqualTo(68));
            Assert.That(report.StateRetirementDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("22 Years and 11 Months"));
        }

        [Test]
        public void ConsidersPrivatePensionSavings()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 30_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            });

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2039, 06, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(58));
            Assert.That(report.PrivateRetirementAge, Is.EqualTo(58));
            Assert.That(report.PrivateRetirementDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.PrivatePensionPot, Is.EqualTo(133_551));
            Assert.That(report.PrivatePensionSafeWithdrawal, Is.EqualTo(5_342));
        }
        
        [Test]
        public void CalculatesReportBasedOnFixedRetirementDate()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                new StatePensionAmountCalculator());
            var status = new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 100_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };
            var report = calc.ReportFor(status, new DateTime(2026, 05, 30));

            Assert.That(report.TargetRetirementDate, Is.EqualTo(new DateTime(2026, 05, 30)));
            Assert.That(report.TargetRetirementAge, Is.EqualTo(45));
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(44));
            Assert.That(report.AnnualStatePension, Is.EqualTo(5_987));
            Assert.That(report.PrivatePensionPot, Is.EqualTo(159_647));
            Assert.That(report.SavingsAtPrivatePensionAge, Is.EqualTo(288_832));
            Assert.That(report.SavingsAt100, Is.EqualTo(410_947));
        }

        [Test]
        public void ReconcileRetirementDateGivenMode_WithCalcMinimumMode()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                new StatePensionAmountCalculator());
            var status = new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 100_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = calc.ReportFor(status);
            Assert.That(report.SavingsAt100, Is.EqualTo(1_612));
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.PrivatePensionPot, Is.EqualTo(150_664));
            Assert.That(report.AnnualStatePension, Is.EqualTo(5_987));
            Assert.That(report.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(339_162));
            
            //validate earliest possible retirement date
            var report2 = calc.ReportFor(status, report.MinimumPossibleRetirementDate);
            Assert.That(report2.SavingsAt100, Is.EqualTo(report.SavingsAt100));
            Assert.That(report2.PrivatePensionPot, Is.EqualTo(report.PrivatePensionPot));
            Assert.That(report2.AnnualStatePension, Is.EqualTo(report.AnnualStatePension));
            Assert.That(report2.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(report.SavingsAtMinimumPossiblePensionAge));
        }

        [Test]
        public void Reconcile_RetirementAgeGivenMode_With_CalcMinimumMode_WhenMinimumComesBeforeTarget()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                new StatePensionAmountCalculator());
            var status = new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 100_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = calc.ReportForTargetAge(status, 50);
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(44));
            Assert.That(report.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(339_162));

            var report2 = calc.ReportFor(status);
            Assert.That(report2.SavingsAt100, Is.EqualTo(1_612));
            Assert.That(report2.MinimumPossibleRetirementDate, Is.EqualTo(report.MinimumPossibleRetirementDate));
            Assert.That(report2.MinimumPossibleRetirementAge, Is.EqualTo(report.MinimumPossibleRetirementAge));
            Assert.That(report2.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(report.SavingsAtMinimumPossiblePensionAge));
        }
        
        [Test]
        public void CalcsMinimumPossibleRetirementDate_WhenMinimumComesAfterTarget()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                new StatePensionAmountCalculator());
            var status = new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 100_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = calc.ReportForTargetAge(status, 42);
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(44));
            Assert.That(report.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(339_162));
        }
    }
}