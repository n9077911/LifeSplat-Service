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
    public class ChildBenefitTests
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly IStatePensionAmountCalculator _statePensionCalculator = new FixedStatePensionAmountCalculator(10_000);
        private readonly IAssumptions _assumptions = Assumptions.SafeWithdrawalNoInflationTake25Assumptions();
        private readonly IPensionAgeCalc _pensionAgeCalc = new PensionAgeCalc();
        private readonly England2020TaxSystem _taxSystem = new();

        [Test]
        public async Task KnowsWhenPeopleCanRetire_ConsideringKids()
        {
            var family =  TestPersons.TwoComplexPeople(_fixedDateProvider.Now(), 30_000)
                .WithSalary(50_000, 30_000)
                .WithEmployeePension(0, 0)
                .WithChild(new DateTime(2015, 1, 1))
                .Family();

            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _pensionAgeCalc, _statePensionCalculator, _taxSystem);

            var report = await calc.ReportForAsync(family, _assumptions);

            Assert.That(report.Persons[0].StepReport.Steps[1].ChildBenefit, Is.EqualTo(92).Within(1));
            Assert.That(report.Persons[1].StepReport.Steps[1].ChildBenefit, Is.EqualTo(0));
        }
        
        [Test]
        public async Task KnowsWhenPeopleCanRetire_ConsideringKids_PensionIsDeductedBeforeCalcingChildBenefit()
        {
            var family = TestPersons.TwoComplexPeople(_fixedDateProvider.Now(), 30_000)
                .WithSalary(100_000, 30_000)
                .WithEmployeePension(50m, 0) //50_000 minus 50% pension is only 30_000
                .WithChild(new DateTime(2015, 1, 1))
                .Family();

            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _pensionAgeCalc, _statePensionCalculator, _taxSystem);

            var report = await calc.ReportForAsync(family, _assumptions);

            Assert.That(report.Persons[0].StepReport.Steps[1].ChildBenefit, Is.EqualTo(92).Within(1));
            Assert.That(report.Persons[1].StepReport.Steps[1].ChildBenefit, Is.EqualTo(0));
        }
        
        [Test]
        public async Task KnowsWhenPeopleCanRetire_ConsideringKids_WhenPartnerHasHigIncome()
        {
            var family = TestPersons.TwoComplexPeople(_fixedDateProvider.Now(), 30_000)
                .WithSalary(30_000, 100_000)
                .WithEmployeePension(50, 0) //50_000 minus 50% pension is only 30_000
                .WithChild(new DateTime(2015, 1, 1))
                .Family();

            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _pensionAgeCalc, _statePensionCalculator, _taxSystem);

            var report = await calc.ReportForAsync(family, _assumptions);

            Assert.That(report.Persons[0].StepReport.Steps[1].ChildBenefit, Is.EqualTo(0));
            Assert.That(report.Persons[1].StepReport.Steps[1].ChildBenefit, Is.EqualTo(0));
        }
    }
}