using System;
using System.Collections.Generic;
using System.Linq;
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

                retirementDateResult.ProcessResults(_now);

                return retirementDateResult;
            }

            var report = ReportFor(family);
            
            report.ProcessResults(_now);
            
            return report;
        }

        private IRetirementReport ReportFor(Family family, bool exitOnceMinCalcd = false, DateTime? givenRetirementDate = null)
        {
            _incomeTaxCalculator = new IncomeTaxCalculator();
            var result = new RetirementReport(_pensionAgeCalc, _incomeTaxCalculator, family, _now, givenRetirementDate, _assumptions);

            var calcdMinimum = false;

            var monthsToDeath = MonthsToDeath(family.PrimaryPerson.Dob, _now);
            for (var month = 1; month <= monthsToDeath; month++)
            {
                ProgressTheResultBy1Month(family, givenRetirementDate, result, calcdMinimum);

                if (givenRetirementDate == null && !calcdMinimum)
                {
                    var retirementReport = result.CopyCalcMinimumMode();
                    if (IsThatEnoughTillDeath(family, retirementReport, month, monthsToDeath))
                    {
                        result.Persons.ForEach((p) => p.FinancialIndependenceDate = result.PrimaryPerson.StepReport.CurrentStep.StepDate);

                        if (exitOnceMinCalcd)
                            return result;
                        calcdMinimum = true;
                    }
                }
            }

            return result;
        }

        private bool IsThatEnoughTillDeath(Family family, RetirementReport report, int month, int monthsToDeath)
        {
            var emergencyFundMet = false;
            var minimumCash = 0;
            for (var m = month+1; m <= monthsToDeath; m++)
            {
                var addMonths = _now.AddMonths(m);
                ProgressTheResultBy1Month(family, null, report, true);
                if (report.EmergencyFund() >= report.RequiredEmergencyFund(addMonths))
                    emergencyFundMet = true;

                if ((report.Investments() + report.EmergencyFund() <= minimumCash) || (emergencyFundMet && report.Investments() <= minimumCash))
                    return false;
            }

            return emergencyFundMet;
        }

        private void ProgressTheResultBy1Month(Family family, DateTime? givenRetirementDate, RetirementReport result, bool calcdMinimum)
        {
            foreach (var person in result.Persons)
            {
                person.StepReport.NewStep(calcdMinimum, result, result.Persons.Count, givenRetirementDate);

                if (person.Retired(calcdMinimum, person.StepReport.CurrentStep.StepDate, givenRetirementDate))
                    person.CrystallisePension();

                person.StepReport.UpdateSpending();
                person.StepReport.UpdatePrivatePension();
                person.StepReport.UpdateGrowth();
                person.StepReport.UpdateStatePensionAmount(_statePensionAmountCalculator, person.StatePensionDate);
                person.StepReport.UpdateSalary(person.MonthlySalaryAfterDeductions);
                person.StepReport.ProcessTaxableIncomeIntoSavings();
            }

            var personWithClaim = result.PersonReportFor(family.PersonWithChildBenefitClaim());
            if (personWithClaim != null)
            {
                var personWithoutClaim = result.PersonReportFor(family.PersonWithoutChildBenefitClaim());
                personWithClaim.StepReport.UpdateChildBenefit(personWithoutClaim);
            }
            
            result.BalanceSavings();
        }

        private int MonthsToDeath(DateTime dob, DateTime now)
        {
            var dateAmount = new DateAmount(now, dob.AddYears(_assumptions.EstimatedDeathAge));
            return dateAmount.TotalMonths();
        }
    }
}