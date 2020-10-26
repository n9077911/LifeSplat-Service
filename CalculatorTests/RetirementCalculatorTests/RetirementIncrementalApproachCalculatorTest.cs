using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Calculator;
using Calculator.Input;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;
using CalculatorTests.Utilities;
using CSharpFunctionalExtensions;
using NUnit.Framework;

namespace CalculatorTests.RetirementCalculatorTests
{
    [TestFixture]
    public class RetirementIncrementalApproachCalculatorTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly StatePensionAmountCalculator _statePensionCalculator = new StatePensionAmountCalculator(new FixedDateProvider(new DateTime(2020, 1, 1)), new TwentyTwentyTaxSystem());
        private readonly IAssumptions _assumptions = Assumptions.SafeWithdrawalNoInflationAssumptions();
        private readonly StubPensionAgeCalc _pensionAgeCalc = new StubPensionAgeCalc(new DateTime(2049, 05, 30));
        private readonly FixedStatePensionAmountCalculator _fixedStatePensionAmountCalculator = new FixedStatePensionAmountCalculator(9110.4m);

        [Test]
        public async Task KnowsWhenASalariedWorkerCanRetire_ThisYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _fixedStatePensionAmountCalculator);
            var report = await calc.ReportForAsync(new Family(new Person {Salary = 30_000, Dob = new DateTime(1981, 05, 30)},
                new []{new SpendingStep(_fixedDateProvider.Now(), 100)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2020, 02, 1)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(38));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("0 Years and 1 Month"));
            Assert.That(report.SavingsAt100, Is.EqualTo(577_990));
            //Knows when someone will not go bankrupt
            Assert.That(report.BankruptDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public async Task KnowsWhenASalariedWorkerCanRetire_NextYear()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = await calc.ReportForAsync(new Family(new Person {Salary = 30_000, Dob = new DateTime(1981, 05, 30)}, new[]{new SpendingStep(_fixedDateProvider.Now(), 2_500)}));
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2021, 12, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(40));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("1 Year and 11 Months"));
            Assert.That(report.SavingsAt100, Is.EqualTo(429_008));
        }

        [Test]
        public async Task KnowsWhenTwoSimplePeopleCanRetire()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);

            var person1 = new Person {Salary = 15_000, Dob = new DateTime(1981, 05, 30)};
            var person2 = new Person {Salary = 15_000, Dob = new DateTime(1981, 05, 30)};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
        
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2038, 04, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(56));
            Assert.That(report.SavingsAt100, Is.EqualTo(5_455));
        }

        [Test]
        public async Task KnowsWhenASalariedWorkerCanRetire_CloseToStatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = await calc.ReportForAsync(new Family(new Person {Salary = 30_000, Dob = new DateTime(1981, 05, 30)},
                new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2048, 08, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(67));
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("28 Years and 7 Months"));
            Assert.That(report.SavingsAt100, Is.EqualTo(3_518));
        }

        [Test]
        public async Task KnowsWhenAWorkerCanRetire_ConsideringExistingSavings()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = await calc.ReportForAsync(new Family(new Person {ExistingSavings = 50_000, Salary = 30_000, Dob = new DateTime(1981, 05, 30)},
                new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2042, 12, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(61));
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("22 Years and 11 Months"));
            Assert.That(report.SavingsAt100, Is.EqualTo(7_282));
        }
        
        [Test]
        public async Task KnowsWhenAWorkerCanRetire_ConsideringEmergencyFundRequirement()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var person = new Person {ExistingSavings = 50_000, Salary = 30_000, Dob = new DateTime(1981, 05, 30), EmergencyFundSpec = new EmergencyFundSpec("10000")};
            var report = await calc.ReportForAsync(new Family(person, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2043, 12, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(62));
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.SavingsAt100, Is.EqualTo(12_640));
        }
        
        [Test]
        public async Task KnowsWhenAWorkerCanRetire_ConsideringEmergencyFundRequirement_And_DownwardSpendingStep()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var person = new Person {ExistingSavings = 50_000, Salary = 30_000, Dob = new DateTime(1981, 05, 30), EmergencyFundSpec = new EmergencyFundSpec("10000")};
            var report = await calc.ReportForAsync(new Family(person,
                new []{new SpendingStep(_fixedDateProvider.Now(), 20_000), new SpendingStep(_fixedDateProvider.Now().AddYears(10), 15_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2037, 01, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(55));
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.SavingsAt100, Is.EqualTo(16_850));
        }
        
        [Test]
        public async Task KnowsWhenAWorkerCanRetire_ConsideringVeryLargeEmergencyFundRequirement()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var person = new Person {ExistingSavings = 50_000, Salary = 30_000, Dob = new DateTime(1981, 05, 30), EmergencyFundSpec = new EmergencyFundSpec("1000000")};
            var report = await calc.ReportForAsync(new Family(person, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(0001, 01, 01)));
            Assert.That(report.SavingsAt100, Is.EqualTo(520_809));
        }

        [Test]
        public async Task KnowsWhenAWorkerCanRetire_ConsideringPrivatePension_CalcMinimumMode()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = await calc.ReportForAsync(new Family(new Person
            {
                ExistingSavings = 50_000, Salary = 30_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            },
                new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2039, 08, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionSafeWithdrawal, Is.EqualTo(5_411));
            Assert.That(report.SavingsAt100, Is.EqualTo(1_946));
        }
        
        [Test]
        public async Task KnowsWhenAHighEarningWomanCanRetire()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, new PensionAgeCalc(), _statePensionCalculator);
            var report = await calc.ReportForAsync(new Family(new Person
            {
                Salary = 100_000, 
                Dob = new DateTime(1981, 05, 30), 
                Sex = Sex.Female,
                ExistingSavings = 50_000,
                ExistingPrivatePension = 100_000,
                EmployerContribution = 0.03m,
                EmployeeContribution = 0.05m,
            }, new []{new SpendingStep(_fixedDateProvider.Now(), 40_000), new SpendingStep(_fixedDateProvider.Now().AddYears(10), 30_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 12, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(49));
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionSafeWithdrawal, Is.EqualTo(14_594));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("10 Years and 11 Months"));
        }
        
        [Test]
        public async Task KnowsWhenAComplexPersonCanRetire_CalculatingAtEndOfTheMonth()
        {
            var eomDate = new FixedDateProvider(new DateTime(2020, 5, 31));
            var calc = new RetirementIncrementalApproachCalculator(eomDate, _assumptions, _pensionAgeCalc,
                new StatePensionAmountCalculator(eomDate, new TwentyTwentyTaxSystem()));
            
            var report = await calc.ReportForAsync(new Family(new Person
            {
                ExistingSavings = 50_000, Salary = 30_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            },
            new []{new SpendingStep(eomDate.Now(), 20_000), new SpendingStep(eomDate.Now(), 18_000)}));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2036, 04, 28)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(54));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionSafeWithdrawal, Is.EqualTo(4_919));
            Assert.That(report.SavingsAt100, Is.EqualTo(5_794));
        }

        [Test]
        public async Task KnowsWhenAWorkerCanRetire_WithTargetRetirementDateGiven()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            var status = new []{new Person
            {
                ExistingSavings = 50_000, Salary = 100_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            }};
            var report = await calc.ReportForAsync(new Family(status, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}), new DateTime(2026, 05, 30));

            Assert.That(report.TargetRetirementDate, Is.EqualTo(new DateTime(2026, 05, 30)));
            Assert.That(report.TargetRetirementAge, Is.EqualTo(45));
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(44));
            Assert.That(report.PrimaryPerson.AnnualStatePension, Is.EqualTo(5_987));
            Assert.That(report.SavingsAt100, Is.EqualTo(410_947));
        }

        [Test]
        public async Task Reconciles_RetirementDateGivenMode_WithCalcMinimumMode()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            var person = new[]{new Person
            {
                ExistingSavings = 50_000, Salary = 100_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            }};

            var report = await calc.ReportForAsync(new Family(person, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}));
            Assert.That(report.SavingsAt100, Is.EqualTo(1_612));
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.PrimaryPerson.AnnualStatePension, Is.EqualTo(5_987));

            //validate earliest possible retirement date
            var report2 = await calc.ReportForAsync(new Family(person, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}), report.FinancialIndependenceDate);
            Assert.That(report2.SavingsAt100, Is.EqualTo(report.SavingsAt100));
            Assert.That(report2.PrimaryPerson.AnnualStatePension, Is.EqualTo(report.PrimaryPerson.AnnualStatePension));
        }

        [Test]
        public async Task Reconcile_RetirementAgeGivenMode_With_CalcMinimumMode_WhenMinimumComesBeforeTarget()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            var status = new Person
            {
                ExistingSavings = 50_000, Salary = 100_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = await calc.ReportForTargetAgeAsync(new []{status}, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}, (Age)50);
            Assert.That(report.SavingsAt100, Is.EqualTo(3_199_745));
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(44));

            var report2 = await calc.ReportForTargetAgeAsync(new []{status}, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}, Maybe<Age>.None);
            Assert.That(report2.SavingsAt100, Is.EqualTo(1_612));
            Assert.That(report2.FinancialIndependenceDate, Is.EqualTo(report.FinancialIndependenceDate));
            Assert.That(report2.FinancialIndependenceAge, Is.EqualTo(report.FinancialIndependenceAge));
        }

        [Test] 
        public async Task KnowsWhenAWorkerCanRetire_WhenMinimumComesAfterTarget()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            var status = new Person
            {
                ExistingSavings = 50_000, Salary = 100_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = await calc.ReportForTargetAgeAsync(new []{status}, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}, (Age)42);
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(44));
            Assert.That(report.SavingsAt100, Is.EqualTo(-525_943));
        }

        [Test]
        public async Task KnowsWhenSomeoneWillGoBankrupt_WhenCalculatingMinimum()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var status = new Person {Salary = 20_000, Dob = new DateTime(1981, 05, 30)};

            var report = await calc.ReportForTargetAgeAsync(new []{status}, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}, Maybe<Age>.None);

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2076, 06, 01)));
            Assert.That(report.BankruptDate, Is.EqualTo(new DateTime(2020, 02, 01)));
        }

        [Test]
        public async Task KnowsWhenSomeoneWillGoBankrupt_WhenTargetDateGiven()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var status = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30)};

            var report = await calc.ReportForTargetAgeAsync(new []{status}, new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)}, Age.Create(42));

            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2034, 06, 01)));
            Assert.That(report.BankruptDate, Is.EqualTo(new DateTime(2026, 09, 01)));
        }
        
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
        
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 07, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(49));
            Assert.That(report.SavingsAt100, Is.EqualTo(43_368));
        } 
        
        //currently one of 2 tests that that inspects the separate person reports.
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_WithSecondPersonOlderThanFirstPerson()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1971, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
        
            Assert.That(report.Persons[0].FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 05, 01)));
            Assert.That(report.Persons[0].FinancialIndependenceAge, Is.EqualTo(48));

            Assert.That(report.Persons[1].FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 05, 01)));
            Assert.That(report.Persons[1].FinancialIndependenceAge, Is.EqualTo(58));
            
            Assert.That(report.SavingsAt100, Is.EqualTo(28_077));
        }

        //currently one of 2 tests that that inspects the separate person reports.
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_WithFirstPersonOlderThanFirstPerson()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1971, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
        
            Assert.That(report.Persons[0].FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 02, 01)));
            Assert.That(report.Persons[0].FinancialIndependenceAge, Is.EqualTo(58));

            Assert.That(report.Persons[1].FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 02, 01)));
            Assert.That(report.Persons[1].FinancialIndependenceAge, Is.EqualTo(48));
            
            Assert.That(report.SavingsAt100, Is.EqualTo(13_485));
        }

        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_WhoChooseToRetire_After_PrivatePensionAge()
        {
            //.44 with target date
            //.35 without target date
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            var startNew = Stopwatch.StartNew();
            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs), new DateTime(2044, 06, 01));
            Console.WriteLine(startNew.Elapsed);
            
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 07, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(49));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.SavingsAt100, Is.EqualTo(6_166_623));
        }
        
        [Test]
        public async Task KnowsWhenTwoComplexPeopleCanRetire_WhoChooseToRetire_Before_PrivatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs), new DateTime(2034, 06, 01));
        
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 07, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(49));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.SavingsAt100, Is.EqualTo(2_214_019));
        }
        
        [Test]
        public async Task KnowsWhenTwoIdenticalPeopleCanRetire()
        {
            //two identical people should be able to retire at the same time as an identical individuals
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                    EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                    EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000)};
            var couple = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
            IEnumerable<Person> personStatuses1 = new[] {person1};
            IEnumerable<SpendingStep> spendingStepInputs1 = new []{new SpendingStep(_fixedDateProvider.Now(), 20_000)};
            var single = await calc.ReportForAsync(new Family(personStatuses1, spendingStepInputs1));

            Assert.That(couple.FinancialIndependenceDate, Is.EqualTo(single.FinancialIndependenceDate));
            Assert.That(couple.FinancialIndependenceAge, Is.EqualTo(single.FinancialIndependenceAge));
            Assert.That(couple.TimeToRetirement.ToString(), Is.EqualTo(single.TimeToRetirement.ToString()));
            Assert.That(couple.SavingsAt100, Is.EqualTo(single.SavingsAt100 * 2).Within(1));
        }

        [Test]
        public async Task KnowsWhenTwoPeopleCanRetire_ConsideringGivenNiContributions()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            
            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), NiContributingYears = 0};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), NiContributingYears = 5};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000)};
            var couple = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
            Assert.That(couple.Persons[0].AnnualStatePension, Is.EqualTo(3_904));
            Assert.That(couple.Persons[1].AnnualStatePension, Is.EqualTo(5_206));
            Assert.That(couple.SavingsAt100, Is.EqualTo(31453));
        }

        [Test]
        public async Task KnowsWhenTwoPeopleCanRetire_ConsidersSteppedSpending()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var now = _fixedDateProvider.Now();

            var spendingSteps = new []{new SpendingStep(now, 40_000),
                new SpendingStep(now.AddYears(5), 50_000),
                new SpendingStep(now.AddYears(10), 30_000),
                new SpendingStep(now.AddYears(20), 20_000)};

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30)};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30)};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingSteps));
            
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2030, 06, 01)));
            Assert.That(report.SavingsAt100, Is.EqualTo(41_611));
        }
        
        [Test]
        public async Task KnowsWhenTwoPeopleCanRetire_WhenTodayDayIsGreaterThanBirthDate()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var birthDay = 10;
            var todayDay = 15;

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1993, 05, birthDay)};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1993, 02, 10)};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new[]{new SpendingStep(new DateTime(2020, 1, todayDay), 20000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
            
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2026, 09, 01)));
            Assert.That(report.SavingsAt100, Is.EqualTo(5_494));
        }
        
        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_ConsideringEmergencyFundRequirement()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("10000")};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("10000")};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 40_000), new SpendingStep(_fixedDateProvider.Now(), 50_000)};
            var report = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
        
            Assert.That(report.Persons.First().StepReport.Steps.First().EmergencyFund, Is.EqualTo(10_000));
            Assert.That(report.Persons.First().StepReport.Steps.Last().EmergencyFund, Is.EqualTo(10_000));
            Assert.That(report.Persons.Last().StepReport.Steps.First().EmergencyFund, Is.EqualTo(10_000));
            Assert.That(report.Persons.Last().StepReport.Steps.Last().EmergencyFund, Is.EqualTo(10_000));
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2035, 09, 01)));
            Assert.That(report.FinancialIndependenceAge, Is.EqualTo(54));
            Assert.That(report.SavingsAt100, Is.EqualTo(58_285));
        } 

        [Test]
        public async Task KnowsWhenTwoComplexWorkingPeopleCanRetire_ConsideringEmergencyFundRequirement_ExpressedAsANumberOfMonths()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000,
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};
            var person2 = new Person {Salary = 50_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("50000")};

            IEnumerable<Person> personStatuses = new[] {person1, person2};
            IEnumerable<SpendingStep> spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 50_000)};
            var reportExpressedAsANumber = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
            
            person1.EmergencyFundSpec =  new EmergencyFundSpec("24m"); //each person spends 25k, so 24m*25k == 50k
            person2.EmergencyFundSpec = new EmergencyFundSpec("24m");
                
            personStatuses = new[] {person1, person2};
            spendingStepInputs = new []{new SpendingStep(_fixedDateProvider.Now(), 50_000)};

            var reportExpressedAsMonths = await calc.ReportForAsync(new Family(personStatuses, spendingStepInputs));
        
            Assert.That(reportExpressedAsANumber.SavingsAt100, Is.EqualTo(reportExpressedAsMonths.SavingsAt100));
        } 
        
        //Written to fix bug
        //whereby person was bankrupt during working years BUT their huge pension hid the fact they had been bankrupt. 
        //This led to the algorithm thinking a person can retire earlier than that could
        [Test]
        public async Task KnowsWhenSomeoneCantRetire_WhenTheyHaveAHugePension()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new Person {Salary = 30_000, Dob = new DateTime(1981, 05, 30), ExistingPrivatePension = 200_000,
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m, EmergencyFundSpec = new EmergencyFundSpec("6m")};

            var report = await calc.ReportForAsync(new Family(new[] {person1}, new []{new SpendingStep(_fixedDateProvider.Now(), 10_000)}));
        
            Assert.That(report.FinancialIndependenceDate, Is.EqualTo(new DateTime(2027, 01, 01)));
            Assert.That(report.SavingsAt100, Is.EqualTo(1_050_439));
        }
    }
}