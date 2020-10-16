using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                var earliestPossibleReport = earliestPossibleTask.Result;

                retirementDateResult.UpdateFinancialIndependenceDate(earliestPossibleReport.FinancialIndependenceDate);

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

                    if (person.Retired(calcdMinimum, person.StepReport.CurrentStep.Date, givenRetirementDate))
                        person.CrystallisePension();
                    
                    person.StepReport.UpdateSpending();
                    person.StepReport.UpdatePrivatePension();
                    person.StepReport.UpdateGrowth();
                    person.StepReport.UpdateStatePensionAmount(_statePensionAmountCalculator, person.StatePensionDate);
                    person.StepReport.UpdateSalary(person.MonthlySalaryAfterDeductions);
                    person.StepReport.ProcessTaxableIncomeIntoSavings();
                }

                var personWithClaim = result.PersonReportFor(family.PersonWithChildBenefitClaim());
                if(personWithClaim != null)
                {
                    var personWithoutClaim = result.PersonReportFor(family.PersonWithoutChildBenefitClaim());
                    personWithClaim.StepReport.UpdateChildBenefit(personWithoutClaim);
                }

                result.BalanceSavings();

                if (givenRetirementDate == null && !calcdMinimum && IsThatEnoughTillDeath(emergencyFund, result, family))
                {
                    result.Persons.ForEach((p)=> p.FinancialIndependenceDate = result.PrimaryPerson.StepReport.CurrentStep.Date);

                    if (exitOnceMinCalcd)
                        return result;
                    calcdMinimum = true;
                }
            }

            return result;
        }

        private bool IsThatEnoughTillDeath(int minimumCash, IRetirementReport result, Family family)
        {
            var primaryStep = result.PrimaryPerson.StepReport.CurrentStep;
            var monthsToDeath = MonthsToDeath(result.PrimaryPerson.Person.Dob, primaryStep.Date);

            var privatePensionAmounts = result.Persons.Select(p => new {p, p.StepReport.CurrentStep.PrivatePensionAmount})
                .ToDictionary(arg => arg.p, arg => arg.PrivatePensionAmount);
            var pensionCrystallised = result.Persons.Select(p => new {p, taken = false})
                .ToDictionary(arg => arg.p, arg => arg.taken);

            var runningInvestments = result.Persons.Select(p => p.StepReport.CurrentStep.Investments).Sum();
            var emergencyFund = result.Persons.Select(p => p.StepReport.CurrentStep.EmergencyFund).Sum();
            var pots = new MoneyPots(runningInvestments, emergencyFund, result.MonthlySpendingAt(primaryStep.Date));

            var emergencyFundMet = false;

            for (var month = 1; month <= monthsToDeath; month++)
            {
                var newIncome = 0m;
                var futureStepDate = primaryStep.Date.AddMonths(month);
                var monthlySpending = result.MonthlySpendingAt(futureStepDate);
                var requiredCash = result.Persons.Select(p => p.Person.EmergencyFundSpec.RequiredEmergencyFund(monthlySpending / result.Persons.Count)).Sum();
                pots = MoneyPots.From(pots, requiredCash);
                pots.AssignSpending(monthlySpending);
                
                newIncome += pots.Investments * _assumptions.MonthlyGrowthRate;

                var biggestIncome = 0m;
                foreach (var person in result.Persons)
                {
                    if (!pensionCrystallised[person] && futureStepDate > person.PrivatePensionDate)
                    {
                        var ltaCharge = LtaChargeRule.Calc(privatePensionAmounts[person], _assumptions.LifeTimeAllowance);
                        privatePensionAmounts[person] += ltaCharge;

                        if (_assumptions.Take25)
                        {
                            var take25Result = new Take25Rule(_assumptions.LifeTimeAllowance).Result(privatePensionAmounts[person]);
                            pots.AssignIncome(take25Result.TaxFreeAmount);
                            privatePensionAmounts[person] = take25Result.NewPensionPot;
                        }

                        pensionCrystallised[person] = true;
                    }

                    var annualisedPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.AnnualGrowthRate;
                    var monthlyPrivatePensionGrowth = privatePensionAmounts[person] * _assumptions.MonthlyGrowthRate;
                    var annualStatePension = person.StepReport.CurrentStep.PredictedStatePensionAnnual;
                    var incomeToPayTaxOn = person.Person.RentalPortfolio.RentalIncome().GetIncomeToPayTaxOn();
                    var totalIncome = annualisedPrivatePensionGrowth + monthlyPrivatePensionGrowth + annualStatePension + incomeToPayTaxOn; 
                    if (totalIncome > biggestIncome)
                        biggestIncome = totalIncome;

                    var taxResult = _incomeTaxCalculator.TaxFor(0, annualisedPrivatePensionGrowth, annualStatePension, person.Person.RentalPortfolio.RentalIncome());

                    var rentalIncome = taxResult.AfterTaxIncomeFor(IncomeType.RentalIncome);
                    newIncome += rentalIncome / _monthly;
                    
                    if (futureStepDate > person.StatePensionDate)
                        newIncome += taxResult.AfterTaxIncomeFor(IncomeType.StatePension) / _monthly;

                    if (futureStepDate > person.PrivatePensionDate)
                        newIncome += monthlyPrivatePensionGrowth - (taxResult.TotalTaxFor(IncomeType.PrivatePension) / _monthly);
                    else
                        privatePensionAmounts[person] += monthlyPrivatePensionGrowth;
                }

                if(family.PersonWithChildBenefitClaim()!=null)
                    newIncome += ChildBenefitCalc.Amount(futureStepDate, family.PersonWithChildBenefitClaim().Children, biggestIncome);
                
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