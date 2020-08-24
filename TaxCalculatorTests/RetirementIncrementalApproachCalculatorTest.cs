using System;
using NUnit.Framework;
using TaxCalculator;
using TaxCalculator.ExternalInterface;
using TaxCalculatorTests.Stubs;

namespace TaxCalculatorTests
{
    //Todo: support paying tax from pension income

    [TestFixture]
    public class RetirementIncrementalApproachCalculatorTest
    {
        private readonly FixedDateProvider _fixedDateProvider = new FixedDateProvider(new DateTime(2020, 1, 1));
        private readonly StatePensionAmountCalculator _statePensionCalculator = new StatePensionAmountCalculator(new FixedDateProvider(new DateTime(2020, 1, 1)), new TwentyTwentyTaxSystem());
        private readonly SafeWithdrawalNoInflationAssumptions _assumptions = new SafeWithdrawalNoInflationAssumptions();
        private readonly StubPensionAgeCalc _pensionAgeCalc = new StubPensionAgeCalc(new DateTime(2049, 05, 30));
        private readonly FixedStatePensionAmountCalculator _fixedStatePensionAmountCalculator = new FixedStatePensionAmountCalculator(9110.4m);

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
            Assert.That(report.SavingsAtPrivatePensionAge, Is.EqualTo(1361));
            Assert.That(report.SavingsAt100, Is.EqualTo(577_990));
            //Knows when someone will not go bankrupt
            Assert.That(report.BankruptDate, Is.EqualTo(DateTime.MaxValue));
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
            Assert.That(report.SavingsAt100, Is.EqualTo(429_008));
        }

        [Test]
        public void KnowsWhenTwoSimpleWorkingPeopleCanRetire()
        {
            
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);

            var person1 = new PersonStatus {Salary = 15_000, Spending = 10_000, Dob = new DateTime(1981, 05, 30)};
            var person2 = new PersonStatus {Salary = 15_000, Spending = 10_000, Dob = new DateTime(1981, 05, 30)};

            var report = calc.ReportFor(new[] {person1, person2});
        
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2038, 04, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(56));
            Assert.That(report.SavingsAt100, Is.EqualTo(5_455));
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
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("28 Years and 7 Months"));
            Assert.That(report.SavingsAt100, Is.EqualTo(3_518));
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
            Assert.That(report.PrimaryPerson.StatePensionAge, Is.EqualTo(68));
            Assert.That(report.PrimaryPerson.StatePensionDate, Is.EqualTo(new DateTime(2049, 05, 30)));
            Assert.That(report.TimeToRetirement.ToString(), Is.EqualTo("22 Years and 11 Months"));
            Assert.That(report.SavingsAt100, Is.EqualTo(7_282));
        }

        [Test]
        public void ConsidersPrivatePensionSavings_CalcMinimum()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var report = calc.ReportFor(new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 30_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            });

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2039, 08, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(133_987));
            Assert.That(report.PrimaryPerson.PrivatePensionSafeWithdrawal, Is.EqualTo(5_359));
            Assert.That(report.SavingsAt100, Is.EqualTo(1_946));
        }
        
        [Test]
        public void HandlesEndOfMonthCalculations()
        {
            var eomDate = new FixedDateProvider(new DateTime(2020, 5, 31));
            var calc = new RetirementIncrementalApproachCalculator(eomDate, _assumptions, _pensionAgeCalc,
                new StatePensionAmountCalculator(eomDate, new TwentyTwentyTaxSystem()));
            
            var report = calc.ReportFor(new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 30_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            });

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2039, 11, 28)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(131_454));
            Assert.That(report.PrimaryPerson.PrivatePensionSafeWithdrawal, Is.EqualTo(5_258));
            Assert.That(report.SavingsAt100, Is.EqualTo(4_235));
        }

        [Test]
        public void CalculatesReportBasedOnFixedRetirementDate()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
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
            Assert.That(report.PrimaryPerson.AnnualStatePension, Is.EqualTo(5_987));
            Assert.That(report.PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(159_647));
            Assert.That(report.SavingsAtPrivatePensionAge, Is.EqualTo(288_832));
            Assert.That(report.SavingsAt100, Is.EqualTo(410_947));
        }

        [Test]
        public void ReconcileRetirementDateGivenMode_WithCalcMinimumMode()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            var status = new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 100_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = calc.ReportFor(status);
            Assert.That(report.SavingsAt100, Is.EqualTo(1_612));
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(150_664));
            Assert.That(report.PrimaryPerson.AnnualStatePension, Is.EqualTo(5_987));
            Assert.That(report.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(339_162));

            //validate earliest possible retirement date
            var report2 = calc.ReportFor(status, report.MinimumPossibleRetirementDate);
            Assert.That(report2.SavingsAt100, Is.EqualTo(report.SavingsAt100));
            Assert.That(report2.PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(report.PrimaryPerson.PrivatePensionPotCombinedAtPrivatePensionAge));
            Assert.That(report2.PrimaryPerson.AnnualStatePension, Is.EqualTo(report.PrimaryPerson.AnnualStatePension));
            Assert.That(report2.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(report.SavingsAtMinimumPossiblePensionAge));
        }

        [Test]
        public void Reconcile_RetirementAgeGivenMode_With_CalcMinimumMode_WhenMinimumComesBeforeTarget()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            var status = new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 100_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = calc.ReportForTargetAge(status, 50);
            Assert.That(report.SavingsAt100, Is.EqualTo(3_199_745));
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
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            var status = new PersonStatus
            {
                ExistingSavings = 50_000, Salary = 100_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
                ExistingPrivatePension = 30_000, EmployerContribution = .03m, EmployeeContribution = .05m
            };

            var report = calc.ReportForTargetAge(status, 42);
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2025, 09, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(44));
            Assert.That(report.SavingsAtMinimumPossiblePensionAge, Is.EqualTo(339_162));
            Assert.That(report.SavingsAt100, Is.EqualTo(-525_943));
        }

        [Test]
        public void KnowsWhenSomeoneWillGoBankrupt_WhenCalculatingMinimum()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var status = new PersonStatus
            {
                Salary = 20_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
            };

            var report = calc.ReportForTargetAge(status);

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2076, 06, 01)));
            Assert.That(report.BankruptDate, Is.EqualTo(new DateTime(2020, 02, 01)));
        }

        [Test]
        public void KnowsWhenSomeoneWillGoBankrupt_WhenTargetDateGiven()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);
            var status = new PersonStatus
            {
                Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30),
            };

            var report = calc.ReportForTargetAge(status, 42);

            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2034, 06, 01)));
            Assert.That(report.BankruptDate, Is.EqualTo(new DateTime(2026, 09, 01)));
        }
        
        [Test]
        public void KnowsWhenTwoComplexWorkingPeopleCanRetire()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            var report = calc.ReportFor(new[] {person1, person2});
        
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2030, 07, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(49));
            Assert.That(report.SavingsAtPrivatePensionAge, Is.EqualTo(371_943));
            Assert.That(report.SavingsAtStatePensionAge, Is.EqualTo(234_105));
            Assert.That(report.SavingsAt100, Is.EqualTo(43_368));
        }
        
        //currently the only test that inspects the separate person reports.
        [Test]
        public void KnowsWhenTwoComplexWorkingPeopleCanRetire_WithDifferentBirthDates()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1971, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            var report = calc.ReportFor(new[] {person1, person2});
        
            Assert.That(report.Persons[0].MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2030, 05, 01)));
            Assert.That(report.Persons[0].MinimumPossibleRetirementAge, Is.EqualTo(48));
            Assert.That(report.Persons[0].SavingsCombinedAtPrivatePensionAge, Is.EqualTo(354_912));
            Assert.That(report.Persons[0].SavingsCombinedAtStatePensionAge, Is.EqualTo(208_095));
            Assert.That(report.Persons[0].PrivatePensionPotAtPrivatePensionAge, Is.EqualTo(179_150));
            Assert.That(report.Persons[0].PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(358_299));
            Assert.That(report.Persons[0].PrivatePensionPotCombinedAtStatePensionAge, Is.EqualTo(358_299));

            Assert.That(report.Persons[1].MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2030, 05, 01)));
            Assert.That(report.Persons[1].MinimumPossibleRetirementAge, Is.EqualTo(58));
            Assert.That(report.Persons[1].SavingsCombinedAtPrivatePensionAge, Is.EqualTo(354_912));
            Assert.That(report.Persons[1].SavingsCombinedAtStatePensionAge, Is.EqualTo(208_095));
            Assert.That(report.Persons[1].PrivatePensionPotAtPrivatePensionAge, Is.EqualTo(179_150));
            Assert.That(report.Persons[1].PrivatePensionPotCombinedAtPrivatePensionAge, Is.EqualTo(358_299));
            Assert.That(report.Persons[1].PrivatePensionPotCombinedAtStatePensionAge, Is.EqualTo(358_299));
            
            Assert.That(report.SavingsAt100, Is.EqualTo(28_077));
        }

        [Test]
        public void KnowsWhenTwoComplexWorkingPeopleCanRetire_WhoChooseToRetire_After_PrivatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            var report = calc.ReportFor(new[] {person1, person2}, new DateTime(2044, 06, 01));
        
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2030, 07, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(49));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.SavingsAtPrivatePensionAge, Is.EqualTo(1_136_843));
            Assert.That(report.SavingsAtStatePensionAge, Is.EqualTo(1_803_063));
            Assert.That(report.SavingsAt100, Is.EqualTo(6_166_623));
            Assert.That(report.PrivatePensionPotAtPrivatePensionAge, Is.EqualTo(446_625));
            Assert.That(report.PrivatePensionPotAtStatePensionAge, Is.EqualTo(587_507));
        }
        
        [Test]
        public void KnowsWhenTwoComplexWorkingPeopleCanRetire_WhoChooseToRetire_Before_PrivatePensionAge()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);

            var person1 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            var report = calc.ReportFor(new[] {person1, person2}, new DateTime(2034, 06, 01));
        
            Assert.That(report.MinimumPossibleRetirementDate, Is.EqualTo(new DateTime(2030, 07, 01)));
            Assert.That(report.MinimumPossibleRetirementAge, Is.EqualTo(49));
            Assert.That(report.PrimaryPerson.PrivatePensionAge, Is.EqualTo(58));
            Assert.That(report.PrimaryPerson.PrivatePensionDate, Is.EqualTo(new DateTime(2039, 05, 30)));
            Assert.That(report.SavingsAtPrivatePensionAge, Is.EqualTo(741_724));
            Assert.That(report.SavingsAtStatePensionAge, Is.EqualTo(801_276));
            Assert.That(report.SavingsAt100, Is.EqualTo(2_214_019));
            Assert.That(report.PrivatePensionPotAtPrivatePensionAge, Is.EqualTo(401_192));
            Assert.That(report.PrivatePensionPotAtStatePensionAge, Is.EqualTo(401_192));
        }
        
        [Test]
        public void KnowsWhenTwoIdenticalPeopleCanRetire()
        {
            //two identical people should be able to retire at the same time as an identical individuals
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc,
                _fixedStatePensionAmountCalculator);

            var person1 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                    EmployeeContribution = 0.05m, EmployerContribution = 0.03m};
            var person2 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), ExistingSavings = 50_000, ExistingPrivatePension = 50_000, 
                    EmployeeContribution = 0.05m, EmployerContribution = 0.03m};

            var couple = calc.ReportFor(new[] {person1, person2});
            var single = calc.ReportFor(new[] {person1});

            Assert.That(couple.MinimumPossibleRetirementDate, Is.EqualTo(single.MinimumPossibleRetirementDate));
            Assert.That(couple.MinimumPossibleRetirementAge, Is.EqualTo(single.MinimumPossibleRetirementAge));
            Assert.That(couple.TimeToRetirement.ToString(), Is.EqualTo(single.TimeToRetirement.ToString()));
            Assert.That(couple.SavingsAt100, Is.EqualTo(single.SavingsAt100 * 2).Within(1));
        }


        [Test]
        public void KnowsWhenTwoIdenticalPeopleCanRetire_ConsideringGivenNiContributions()
        {
            var calc = new RetirementIncrementalApproachCalculator(_fixedDateProvider, _assumptions, _pensionAgeCalc, _statePensionCalculator);
            
            var person1 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), NiContributingYears = 0};
            var person2 = new PersonStatus {Salary = 50_000, Spending = 20_000, Dob = new DateTime(1981, 05, 30), NiContributingYears = 5};

            var couple = calc.ReportFor(new[] {person1, person2});
            Assert.That(couple.Persons[0].AnnualStatePension, Is.EqualTo(3_904));
            Assert.That(couple.Persons[1].AnnualStatePension, Is.EqualTo(5_206));
            Assert.That(couple.Persons[0].PrivatePensionPotCombinedAtStatePensionAge, Is.EqualTo(0));
            Assert.That(couple.Persons[1].PrivatePensionPotCombinedAtStatePensionAge, Is.EqualTo(0));
            Assert.That(couple.SavingsAt100, Is.EqualTo(31453));
        }
    }
}