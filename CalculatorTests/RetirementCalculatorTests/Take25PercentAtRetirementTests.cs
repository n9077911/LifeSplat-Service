using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calculator;
using Calculator.Input;
using Calculator.TaxSystem;
using CalculatorTests.Utilities;
using NUnit.Framework;

namespace CalculatorTests.RetirementCalculatorTests
{
    [TestFixture]
    public class Take25PercentAtRetirementTests
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly StatePensionAmountCalculator _statePensionCalculator = new StatePensionAmountCalculator(new FixedDateProvider(new DateTime(2020, 1, 1)), new TwentyTwentyTaxSystem());
        private readonly IAssumptions _assumptions = Assumptions.SafeWithdrawalNoInflationTake25Assumptions();
        private readonly StubPensionAgeCalc _pensionAgeCalc = new StubPensionAgeCalc(new DateTime(2049, 05, 30));
        
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_NoGivenRetirementDate_RetirementAfterPrivatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000,
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> stepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 70_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, stepInputs));
            
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2055, 03, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(73));
            Assert.That(report.SavingsAt100, Is.EqualTo(115_546));

        }
        
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_NoGivenRetirementDate_RetirementBeforePrivatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000,
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 50_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
            
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2037, 03, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(55));
            Assert.That(report.SavingsAt100, Is.EqualTo(115_710));
        }
        
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_WithGivenRetirementDate_RetirementAfterPrivatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000,
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 50_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs), new DateTime(2050, 03, 01));
            
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2037, 03, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(55));
            Assert.That(report.SavingsAt100, Is.EqualTo(4_313_451));
        }
        
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_WithGivenRetirementDate_RetirementBeforePrivatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000,
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 41_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs), new DateTime(2035, 03, 01));
            
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2032, 08, 01)));
            Assert.That(report.SavingsAt100, Is.EqualTo(1_462_789));
        }
    }
}