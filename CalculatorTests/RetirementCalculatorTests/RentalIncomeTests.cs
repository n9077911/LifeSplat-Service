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
    public class RentalIncomeTests
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly IStatePensionAmountCalculator _statePensionCalculator = new FixedStatePensionAmountCalculator(10_000);
        private readonly IAssumptions _assumptions = Assumptions.SafeWithdrawalNoInflationTake25Assumptions();
        private readonly StubPensionAgeCalc _pensionAgeCalc = new StubPensionAgeCalc(new DateTime(2049, 05, 30));
        private readonly England2020TaxSystem _taxSystem = new England2020TaxSystem();


        [TestCase]
        public async Task KnowsWhenTwoComplexPeopleCanRetire_WithRentalIncome()
        {
            //can retire early - based mainly on rental income
            await KnowsWhenTwoComplexPeopleCanRetire_WithRentalIncome(20_000, null, new DateTime(2020, 2, 01), 2_085_505);
            //can retire early - no target date
            await KnowsWhenTwoComplexPeopleCanRetire_WithRentalIncome(60_000, null, new DateTime(2034, 11, 01), 103_319);
            //can retire early - target date shortly after minimum
            await KnowsWhenTwoComplexPeopleCanRetire_WithRentalIncome(60_000, new DateTime(2036, 04, 01), new DateTime(2034, 11, 01), 677_278);
            //can retire early - target date in old age
            await KnowsWhenTwoComplexPeopleCanRetire_WithRentalIncome(60_000, new DateTime(2060, 04, 01), new DateTime(2034, 11, 01), 6_082_580);
            //can retire early - target date leads to bankruptcy
            await KnowsWhenTwoComplexPeopleCanRetire_WithRentalIncome(60_000, new DateTime(2028, 04, 01), new DateTime(2034, 11, 01), -826_393);
        }

        private async Task KnowsWhenTwoComplexPeopleCanRetire_WithRentalIncome(int spending, DateTime? targetRetirement, DateTime minimumPossible, int savings)
        {
            var twoComplexPeople = TestPersons.TwoComplexPeople(_fixedDateProvider.Now(), spending)
                .WithRental(0, 10_000, 3_000, 2000)
                .WithRental(0, 10_000, 3_000, 2000)
                .WithRental(1, 10_000, 3_000, 2000)
                .WithRental(1, 10_000, 3_000, 2000)
                .Family();

            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _pensionAgeCalc, _statePensionCalculator, _taxSystem);

            var report = await calc.ReportForAsync(twoComplexPeople, _assumptions, targetRetirement);

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(minimumPossible));
            Assert.That(report.SavingsAt100, Is.EqualTo(savings));
        }
    }
}