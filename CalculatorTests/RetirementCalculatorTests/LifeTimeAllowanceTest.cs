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
    [TestFixture]
    public class LifeTimeAllowanceTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly IStatePensionAmountCalculator _statePensionCalculator = new FixedStatePensionAmountCalculator(10_000);
        private readonly IAssumptions _assumptions = Assumptions.SafeWithdrawalNoInflationTake25Assumptions();
        private readonly IPensionAgeCalc _pensionAgeCalc = new PensionAgeCalc();
        private England2020TaxSystem _taxSystem;

        [Test]
        public async Task KnowsWhenTwoComplexPeopleCanRetire_GivenTheyAreAboveTheLTA()
        {
            _taxSystem = new England2020TaxSystem();
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator, _taxSystem);

            var family = TestPersons.TwoComplexPeople_WithPension(_fixedDateProvider.Now(), 50_000, 1000_000).Family();
            var report = await calc.ReportForAsync(family);

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2031, 11, 01)));
            Assert.That(report.Persons[0].PrivatePensionPotCrystallisationDate, Is.EqualTo(new DateTime(2039, 06, 01)));
            
            Assert.That(report.Persons[0].LifeTimeAllowanceTaxCharge, Is.EqualTo(285_544));
            Assert.That(report.Persons[0].Take25LumpSum, Is.EqualTo(268_275));
            Assert.That(report.Persons[0].PrivatePensionPotAtCrystallisationAge, Is.EqualTo(1_661_456));
            Assert.That(report.Persons[0].PrivatePensionPotBeforeCrystallisation, Is.EqualTo(2_215_275));
            
            Assert.That(report.Persons[1].LifeTimeAllowanceTaxCharge, Is.EqualTo(392_445));
            Assert.That(report.Persons[1].Take25LumpSum, Is.EqualTo(268_275));
            Assert.That(report.Persons[1].PrivatePensionPotAtCrystallisationAge, Is.EqualTo(1_982_161));
            Assert.That(report.Persons[1].PrivatePensionPotBeforeCrystallisation, Is.EqualTo(2_642_881));
            
            Assert.That(report.SavingsAt100, Is.EqualTo(8_463_709));
        }
        
        [Test]
        public async Task KnowsWhenTwoComplexPeopleCanRetire_GivenTheyAreBelowTheLTA()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator, _taxSystem);

            var family = TestPersons.TwoComplexPeople_WithPension(_fixedDateProvider.Now(), 50_000, 0).Family();
            var report = await calc.ReportForAsync(family);

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2039, 05, 01)));
            Assert.That(report.Persons[0].PrivatePensionPotCrystallisationDate, Is.EqualTo(new DateTime(2039, 06, 01)));
            Assert.That(report.Persons[0].PrivatePensionCrystallisationAge, Is.EqualTo(58));
            
            Assert.That(report.Persons[1].PrivatePensionPotCrystallisationDate, Is.EqualTo(new DateTime(2044, 01, 01)));
            Assert.That(report.Persons[1].PrivatePensionCrystallisationAge, Is.EqualTo(58));
            
            Assert.That(report.Persons[0].LifeTimeAllowanceTaxCharge, Is.EqualTo(0));
            Assert.That(report.Persons[0].Take25LumpSum, Is.EqualTo(28_881));
            Assert.That(report.Persons[0].PrivatePensionPotAtCrystallisationAge, Is.EqualTo(86_642));
            Assert.That(report.Persons[0].PrivatePensionPotBeforeCrystallisation, Is.EqualTo(115_523));
            
            Assert.That(report.Persons[1].LifeTimeAllowanceTaxCharge, Is.EqualTo(0));
            Assert.That(report.Persons[1].Take25LumpSum, Is.EqualTo(34_455));
            Assert.That(report.Persons[1].PrivatePensionPotAtCrystallisationAge, Is.EqualTo(103_366));
            Assert.That(report.Persons[1].PrivatePensionPotBeforeCrystallisation, Is.EqualTo(137_822));
            
            Assert.That(report.SavingsAt100, Is.EqualTo(115_260));
        }
    }
}