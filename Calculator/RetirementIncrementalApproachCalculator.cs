using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Calculator.ExternalInterface;
using Calculator.Input;
using Calculator.Output;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;

namespace Calculator
{
    /// <summary>
    /// Generates a retirement report detailing when a user can retire by iterating through a users future life
    /// </summary>
    public class RetirementIncrementalApproachCalculator : IRetirementCalculator
    {
        private readonly IAssumptions _assumptions;
        private readonly IPensionAgeCalc _pensionAgeCalc;
        private readonly IStatePensionAmountCalculator _statePensionAmountCalculator;
        private readonly decimal _monthly = 12;
        private readonly DateTime _now;
        private IncomeTaxCalculator _incomeTaxCalculator;

        public RetirementIncrementalApproachCalculator(IDateProvider dateProvider,
            IAssumptions assumptions, IPensionAgeCalc pensionAgeCalc,
            IStatePensionAmountCalculator statePensionAmountCalculator)
        {
            _assumptions = assumptions;
            _pensionAgeCalc = pensionAgeCalc;
            _statePensionAmountCalculator = statePensionAmountCalculator;
            _now = dateProvider.Now();
        }

        public async Task<IRetirementReport> ReportForTargetAgeAsync(IEnumerable<Person> personStatus, IEnumerable<SpendingStep> spendingStepInputs, int? retirementAge = null)
        {
            var retirementDate = retirementAge.HasValue && retirementAge.Value != 0 ? personStatus.First().Dob.AddYears(retirementAge.Value) : (DateTime?) null;
            return await ReportForAsync(new Family(personStatus, spendingStepInputs), retirementDate);
        }

        public async Task<IRetirementReport> ReportForAsync(Family family, DateTime? givenRetirementDate = null)
        {
            if (givenRetirementDate != null)
            {
                var retirementDateTask = Task.Run(() => ReportFor(family, false, givenRetirementDate));
                var earliestPossibleTask = Task.Run(() => ReportFor(family, true));

                await retirementDateTask;
                await earliestPossibleTask;

                var retirementDateResult = retirementDateTask.Result;
                var earliestPossible = earliestPossibleTask.Result;

                retirementDateResult.UpdateMinimumPossibleInfo(earliestPossible.MinimumPossibleRetirementDate, earliestPossible.SavingsAtMinimumPossiblePensionAge);

                retirementDateResult.ProcessResults(givenRetirementDate, _now);

                return retirementDateResult;
            }

            var report = ReportFor(family);
            report.ProcessResults(givenRetirementDate, _now);
            return report;
        }

        private IRetirementReport ReportFor(Family family, bool exitOnceMinCalcd = false, DateTime? givenRetirementDate = null)
        {
            _incomeTaxCalculator = new IncomeTaxCalculator();
            var result = new RetirementReport(_pensionAgeCalc, _incomeTaxCalculator, family, _now, givenRetirementDate, _assumptions);

            var emergencyFund = 0;

            var calcdMinimum = false;

            for (var month = 1; month <= MonthsToDeath(family.PrimaryPerson.Dob, _now); month++)
            {
                foreach (var person in result.Persons)
                {
                    person.StepReport.NewStep(calcdMinimum, result, result.Persons.Count, givenRetirementDate);

                    if (person.Take25WhenRetired(calcdMinimum, person.StepReport.CurrentStep.Date, givenRetirementDate))
                        person.Take25();
                    
                    person.StepReport.UpdateSpending();
                    person.StepReport.UpdatePrivatePension();
                    person.StepReport.UpdateGrowth();
                    person.StepReport.UpdateStatePensionAmount(_statePensionAmountCalculator, person.StatePensionDate);
                    person.StepReport.UpdateSalary(person.MonthlySalaryAfterDeductions);
                    person.StepReport.ProcessTaxableIncomeIntoSavings();
                }

                result.BalanceSavings();

                if (givenRetirementDate == null && !calcdMinimum && IsThatEnoughTillDeath(emergencyFund, result))
                {
                    foreach (var resultPerson in result.Persons)
                    {
                        resultPerson.MinimumPossibleRetirementDate = result.PrimaryPerson.StepReport.CurrentStep.Date;
                        resultPerson.SavingsAtMinimumPossiblePensionAge = Convert.ToInt32(result.Persons.Select(p => p.StepReport.CurrentStep.Investments).Sum());
                    }

                    if (exitOnceMinCalcd)
                        return result;
                    calcdMinimum = true;
                }
            }

            return result;
        }

        private bool IsThatEnoughTillDeath(int minimumCash, IRetirementReport result)
        {
            var primaryStep = result.PrimaryPerson.StepReport.CurrentStep;
            var monthsToDeath = MonthsToDeath(result.PrimaryPerson.Person.Dob, primaryStep.Date);

            var privatePensionAmounts = result.Persons.Select(p => new {p, p.StepReport.CurrentStep.PrivatePensionAmount})
                .ToDictionary(arg => arg.p, arg => arg.PrivatePensionAmount);
            var taken25 = result.Persons.Select(p => new {p, taken = false})
                .ToDictionary(arg => arg.p, arg => arg.taken);

            var runningInvestments = result.Persons.Select(p => p.StepReport.CurrentStep.Investments).Sum();
            var emergencyFund = result.Persons.Select(p => p.StepReport.CurrentStep.EmergencyFund).Sum();
            var pots = new MoneyPots(runningInvestments, emergencyFund, result.MonthlySpendingAt(primaryStep.Date));

            var emergencyFundMet = false;

            for (var month = 1; month <= monthsToDeath; month++)
            {
                var newIncome = 0m;
                var monthlySpending = result.MonthlySpendingAt(primaryStep.Date.AddMonths(month));
                var requiredCash = result.Persons.Select(p => p.Person.EmergencyFundSpec.RequiredEmergencyFund(monthlySpending / result.Persons.Count)).Sum();
                pots = MoneyPots.From(pots, requiredCash);
                pots.AssignSpending(monthlySpending);
                
                newIncome += pots.Investments * _assumptions.MonthlyGrowthRate;

                foreach (var person in result.Persons)
                {
                    if (_assumptions.Take25 && !taken25[person] && primaryStep.Date.AddMonths(month) > person.PrivatePensionDate)
                    {
                        var take25Result = new Take25Rule(_assumptions.LifeTimeAllowance).Result(privatePensionAmounts[person]);

                        pots.AssignIncome(take25Result.TaxFreeAmount);
                        privatePensionAmounts[person] = take25Result.NewPensionPot;
                        taken25[person] = true;
                    }

                    var annualisedPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.AnnualGrowthRate;
                    var monthlyPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.MonthlyGrowthRate;
                    var annualStatePension = person.StepReport.CurrentStep.PredictedStatePensionAnnual;

                    var taxResult = _incomeTaxCalculator.TaxFor(0, annualisedPrivatePensionGrowth, annualStatePension, person.Person.RentalPortfolio.RentalIncome());

                    var rentalIncome = taxResult.AfterTaxIncomeFor(IncomeType.RentalIncome);
                    newIncome += rentalIncome / _monthly;
                    
                    if (primaryStep.Date.AddMonths(month) > person.StatePensionDate)
                        newIncome += taxResult.AfterTaxIncomeFor(IncomeType.StatePension) / _monthly;

                    if (primaryStep.Date.AddMonths(month) > person.PrivatePensionDate)
                        newIncome += monthlyPrivatePensionGrowth - (taxResult.TotalTaxFor(IncomeType.PrivatePension) / _monthly);
                    else
                        privatePensionAmounts[person] += monthlyPrivatePensionGrowth;
                }

                pots.AssignIncome(newIncome);
                pots.Rebalance();

                if (pots.EmergencyFund >= requiredCash)
                    emergencyFundMet = true;

                if ((pots.Investments + pots.EmergencyFund <= minimumCash) || (emergencyFundMet && pots.Investments  <= minimumCash))
                    return false;
            }

            return emergencyFundMet;
        }

        private int MonthsToDeath(DateTime dob, DateTime now)
        {
            var dateAmount = new DateAmount(now, dob.AddYears(_assumptions.EstimatedDeathAge));
            return dateAmount.TotalMonths();
        }
    }
}