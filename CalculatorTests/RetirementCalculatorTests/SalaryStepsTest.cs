using System;
using System.Threading.Tasks;
using Calculator;
using Calculator.Input;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;
using CalculatorTests.Utilities;
using NUnit.Framework;

namespace CalculatorTests.RetirementCalculatorTests
{
    public class SalaryStepsTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly IStatePensionAmountCalculator _statePensionCalculator = new FixedStatePensionAmountCalculator(10_000);
        private readonly IAssumptions _assumptions = Assumptions.SafeWithdrawalNoInflationTake25Assumptions();
        private readonly IPensionAgeCalc _pensionAgeCalc = new PensionAgeCalc();
        private readonly TwentyTwentyTaxSystem _taxSystem = new TwentyTwentyTaxSystem();

        [Test]
        public async Task KnowsWhenTwoPeopleCanRetire_ConsideringSalarySteps()
        {
            var family = TestPersons.TwoComplexPeople(_fixedDateProvider.Now(), 30_000)
                .WithSalary(30_000, 100_000)
                .WithSalaryStep(0, 40, 40_000)
                .WithSalaryStep(0, 42, 50_000)
                .WithSalaryStep(1, 50, 50_000)
                .Family();    
            
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator, _taxSystem);

            var report = await calc.ReportForAsync(family);

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2025, 9, 1)));
            Assert.That(report.SavingsAt100, Is.EqualTo(151_343));
        }
        
        [Test]
        public async Task SalaryStepsFeedIntoNationalInsuranceCalculation()
        {
            var family = TestPersons.OnePerson(_fixedDateProvider.Now(), 35_000)
                .WithSavings(0, 100_000)
                .WithSalary(0)
                .WithSalaryStep(0, 41, 50_000)
                .Family();

            var realStatePensionCalculator = new StatePensionAmountCalculator();
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, realStatePensionCalculator, _taxSystem);

            var report = await calc.ReportForAsync(family);

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2066, 2, 1)));
            Assert.That(report.Persons[0].NiContributingYears, Is.EqualTo(35));
            Assert.That(report.SavingsAt100, Is.EqualTo(50_254));
        }
    }
}