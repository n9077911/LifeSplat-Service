using System;
using System.Diagnostics;
using Calculator.Input;
using Calculator.TaxSystem;

namespace Calculator
{
    /// <summary>
    /// Represents 1 month of a persons future life.
    /// Used by RetirementIncrementalApproachCalculator to keep track of a persons finances as the calculator iterates through the persons future. 
    /// </summary>
    [DebuggerDisplay("{Date} : {Investments}")]
    public class Step
    {
        private readonly Step _previousStep;
        private readonly Person _person;
        private readonly bool _calcdMinimum;
        private readonly IAssumptions _assumptions;
        private readonly DateTime _privatePensionDate;
        private readonly DateTime? _givenRetirementDate;
        private decimal _monthly = 12m;

        private decimal _preTaxSalary = 0;
        private decimal _annualPreTaxPrivatePensionIncome = 0;
        private decimal _monthlyPreTaxPrivatePensionIncome = 0;
        private decimal _preTaxStatePensionIncome = 0;
        private MoneyPots Pots { get; }

        public Step(Step previousStep, DateTime stepDate, Person person, bool calcdMinimum, IAssumptions assumptions, 
            DateTime privatePensionDate, decimal spending, DateTime? givenRetirementDate = null)
        {
            Date = stepDate;
            PrivatePensionAmount = previousStep.PrivatePensionAmount;
            _previousStep = previousStep;
            _person = person;
            _calcdMinimum = calcdMinimum;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            Spending = spending;
            _givenRetirementDate = givenRetirementDate;
            Pots = MoneyPots.From(previousStep.Pots, _person.EmergencyFundSpec.RequiredEmergencyFund(Spending));
        }

        private Step(DateTime now, int existingSavings, int existingPrivatePension, EmergencyFundSpec emergencyFundSpec, decimal personMonthlySpending)
        {
            Date = now;

            var requiredCashSavings = emergencyFundSpec.RequiredEmergencyFund(personMonthlySpending);
            Pots = new MoneyPots(requiredCashSavings);
            Pots.AssignIncome(existingSavings);
            
            PrivatePensionAmount = existingPrivatePension;
            Spending = personMonthlySpending;
        }

        public static Step CreateInitialStep(DateTime now, int existingSavings, int existingPrivatePension, EmergencyFundSpec emergencyFundSpec, decimal personMonthlySpending)
        {
            return new Step(now, existingSavings, existingPrivatePension, emergencyFundSpec, personMonthlySpending);
        }

        public DateTime Date { get; private set; }
        public decimal PredictedStatePensionAnnual { get; private set; }
        public int NiContributingYears { get; private set; }
        public decimal Growth { get; private set; }
        public decimal AfterTaxSalary { get; private set; }
        public decimal AfterTaxRentalIncome { get; private set; }
        public decimal AfterTaxStatePension { get; private set; }
        public decimal AfterTaxPrivatePensionIncome { get; private set; }
        
        public decimal Spending { get; }
        public decimal PrivatePensionAmount { get; private set; }
        public decimal Investments => Pots.Investments;
        public decimal EmergencyFund => Pots.EmergencyFund;

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate)
        {
            if (PersonHasQuitWork())
            {
                PredictedStatePensionAnnual = _previousStep.PredictedStatePensionAnnual;
                NiContributingYears = _previousStep.NiContributingYears;
            }
            else
            {
                var predictedStatePensionAnnual = statePensionAmountCalculator.Calculate(_person, Date);
                NiContributingYears = predictedStatePensionAnnual.ContributingYears;
                PredictedStatePensionAnnual = Convert.ToInt32(predictedStatePensionAnnual.Amount);
            }

            if (Date > personStatePensionDate)
            {
                _preTaxStatePensionIncome = PredictedStatePensionAnnual/_monthly;
            }
        }

        private bool PersonHasQuitWork() => _givenRetirementDate.HasValue ? Date > _givenRetirementDate : _calcdMinimum;
        
        public void UpdateGrowth()
        {
            var growth = Math.Max(Pots.Investments * _assumptions.MonthlyGrowthRate, 0m);
            Growth = growth;
            Pots.AssignIncome(growth);
        }

        public void UpdatePrivatePension()
        {
            var privatePensionGrowth = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;

            if (Date >= _privatePensionDate && PersonHasQuitWork())
            {
                //must be annualised (using the monthly figure and multiplying by 12 won't work as 12*monthlyRate != annualRate - because the monthly rate assumes compounding!
                _annualPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.AnnualGrowthRate;
                _monthlyPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;
            }
            else
                PrivatePensionAmount += privatePensionGrowth;
            
            if (!PersonHasQuitWork())
                PrivatePensionAmount += (_person.Salary / _monthly) * (_person.EmployeeContribution + _person.EmployerContribution);
        }

        public void UpdateSalary(decimal preTaxSalary)
        {
            if (!PersonHasQuitWork())
            {
                _preTaxSalary = preTaxSalary;
            }
        }

        public void UpdateSpending()
        {
            Pots.AssignSpending(Spending);
        }

        public void SetSavings(decimal savings)
        {
            Pots.Investments = savings;
        }
        
        public void SetEmergencyFund(decimal emergencyFund)
        {
            Pots.EmergencyFund = emergencyFund;
        }

        public void PayTaxAndBankTheRemainder()
        {
            //Simplification - calculate tax for the whole year then divide it for the month
            //this will not be accurate on years where someone quits work or starts receiving a pension.
            //In that case the fact they work\receive pension for a partial year would mean their real tax bill is less that calculated here
            var incomeTaxCalculator = new IncomeTaxCalculator();
            var afterTax = incomeTaxCalculator.TaxFor(_preTaxSalary*12, _annualPreTaxPrivatePensionIncome, _preTaxStatePensionIncome*12, _person.RentalPortfolio.RentalIncome());
            AfterTaxSalary = afterTax.AfterTaxIncomeFor(IncomeType.Salary)/12;
            AfterTaxPrivatePensionIncome = _monthlyPreTaxPrivatePensionIncome - (afterTax.TotalTaxFor(IncomeType.PrivatePension)/12);
            AfterTaxStatePension = afterTax.AfterTaxIncomeFor(IncomeType.StatePension)/12;
            AfterTaxRentalIncome = afterTax.AfterTaxIncomeFor(IncomeType.RentalIncome)/12;

            var newIncome = AfterTaxSalary + AfterTaxPrivatePensionIncome + AfterTaxStatePension + AfterTaxRentalIncome;

            Pots.AssignIncome(newIncome);
            Pots.Rebalance();
        }

        public Take25Result Take25()
        {
            var take25Result = new Take25Rule(_assumptions.LifeTimeAllowance).Result(PrivatePensionAmount);

            Pots.AssignIncome(take25Result.TaxFreeAmount);
            PrivatePensionAmount = take25Result.NewPensionPot;

            return take25Result;
        }
    }
}