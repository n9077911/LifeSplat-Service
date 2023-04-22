using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Calculator.Input;
using Calculator.Output;
using Calculator.StatePensionCalculator;
using Calculator.TaxSystem;

namespace Calculator
{
    /// <summary>
    /// Represents 1 month of a persons future life.
    /// Used by RetirementIncrementalApproachCalculator to keep track of a persons finances as the calculator iterates through the persons future. 
    /// </summary>
    [DebuggerDisplay("{StepDate} : {Investments}")]
    public class Step
    {
        private readonly Step _previousStep;
        private readonly Person _person;
        private readonly bool _calcdMinimum;
        private readonly IAssumptions _assumptions;
        private readonly DateTime _privatePensionDate;
        private readonly DateTime? _givenRetirementDate;
        private readonly ITaxSystem _taxSystem;
        private decimal _monthly = 12m;

        private decimal _preTaxSalary = 0;
        private decimal _annualPreTaxPrivatePensionIncome = 0;
        private decimal _monthlyPreTaxPrivatePensionIncome = 0;
        private decimal _preTaxStatePensionIncome = 0;
        private MoneyPots Pots { get; }

        public Step(Step previousStep, DateTime stepStepDate, Person person, bool calcdMinimum, IAssumptions assumptions,
            DateTime privatePensionDate, decimal spending, ITaxSystem taxSystem, DateTime? givenRetirementDate = null, MoneyPots pots = null)
        {
            StepDate = stepStepDate;
            PrivatePensionAmount = previousStep.PrivatePensionAmount;
            Spending = spending;
            NiContributingYears = previousStep.NiContributingYears;
            _previousStep = previousStep;
            _person = person;
            _calcdMinimum = calcdMinimum;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            _givenRetirementDate = givenRetirementDate;
            _taxSystem = taxSystem;
            Pots = pots ?? MoneyPots.From(previousStep.Pots, _person.EmergencyFundSpec.RequiredEmergencyFund(Spending));
        }

        private Step(DateTime now, int niContributingYearsSoFar, int existingSavings, int existingPrivatePension, EmergencyFundSpec emergencyFundSpec, decimal personMonthlySpending)
        {
            StepDate = now;

            NiContributingYears = niContributingYearsSoFar;
            var requiredCashSavings = emergencyFundSpec.RequiredEmergencyFund(personMonthlySpending);
            Pots = new MoneyPots(requiredCashSavings);
            Pots.AssignIncome(existingSavings);

            PrivatePensionAmount = existingPrivatePension;
            Spending = personMonthlySpending;
        }

        public static Step CreateInitialStep(DateTime now, int niContributingYearsSoFar, int existingSavings, int existingPrivatePension, EmergencyFundSpec emergencyFundSpec,
            decimal personMonthlySpending)
        {
            return new Step(now, niContributingYearsSoFar, existingSavings, existingPrivatePension, emergencyFundSpec, personMonthlySpending);
        }

        private Step(Step previousStep, in DateTime stepDate, Person person, in bool calcdMinimum, IAssumptions assumptions, in DateTime privatePensionDate, in decimal spending, DateTime? 
            givenRetirementDate, MoneyPots pots, in decimal predictedStatePensionAnnual, in int niContributingYears, in decimal growth, in decimal childBenefit, in decimal afterTaxSalary, 
            in decimal afterTaxRentalIncome, in decimal afterTaxStatePension, in decimal afterTaxPrivatePensionIncome, in decimal privatePensionAmount,
            decimal preTaxSalary, decimal annualPreTaxPrivatePensionIncome, decimal monthlyPreTaxPrivatePensionIncome, decimal preTaxStatePensionIncome, ITaxSystem taxSystem)
        {
            StepDate = stepDate;
            Spending = spending;
            Pots = pots;
            PredictedStatePensionAnnual = predictedStatePensionAnnual;
            NiContributingYears = niContributingYears;
            Growth = growth;
            ChildBenefit = childBenefit;
            AfterTaxSalary = afterTaxSalary;
            AfterTaxRentalIncome = afterTaxRentalIncome;
            AfterTaxStatePension = afterTaxStatePension;
            AfterTaxPrivatePensionIncome = afterTaxPrivatePensionIncome;
            PrivatePensionAmount = privatePensionAmount;
            _previousStep = previousStep;
            _person = person;
            _calcdMinimum = calcdMinimum;
            _assumptions = assumptions;
            _privatePensionDate = privatePensionDate;
            _givenRetirementDate = givenRetirementDate;
            _preTaxSalary = preTaxSalary;
            _annualPreTaxPrivatePensionIncome = annualPreTaxPrivatePensionIncome;
            _monthlyPreTaxPrivatePensionIncome = monthlyPreTaxPrivatePensionIncome;
            _preTaxStatePensionIncome = preTaxStatePensionIncome;
            _taxSystem = taxSystem;
        }

        public Step CopyForCalcMinimumMode()
        {
            return new Step(_previousStep, StepDate, _person, _calcdMinimum, _assumptions, _privatePensionDate, Spending, _givenRetirementDate, Pots.Copy(), 
                PredictedStatePensionAnnual, NiContributingYears, Growth, ChildBenefit, AfterTaxSalary, AfterTaxRentalIncome, AfterTaxStatePension, AfterTaxPrivatePensionIncome, PrivatePensionAmount,
                _preTaxSalary, _annualPreTaxPrivatePensionIncome, _monthlyPreTaxPrivatePensionIncome, _preTaxStatePensionIncome, _taxSystem); 
        }

        public DateTime StepDate { get; private set; }
        public decimal PredictedStatePensionAnnual { get; private set; }
        public int NiContributingYears { get; private set; }
        public decimal Growth { get; private set; }
        public decimal ChildBenefit { get; private set; }
        public decimal AfterTaxSalary { get; private set; }
        public decimal AfterTaxRentalIncome { get; private set; }
        public decimal AfterTaxStatePension { get; private set; }
        public decimal AfterTaxPrivatePensionIncome { get; private set; }
        public decimal PrivatePensionAmount { get; private set; }
        public decimal Spending { get; }
        
        public decimal Investments => Pots.Investments;
        public decimal EmergencyFund => Pots.EmergencyFund;

        public void UpdateStatePensionAmount(IStatePensionAmountCalculator statePensionAmountCalculator, DateTime personStatePensionDate, Money monthlySalary, DateTime now)
        {
            if (PersonHasQuitWork())
            {
                PredictedStatePensionAnnual = _previousStep.PredictedStatePensionAnnual;
                NiContributingYears = _previousStep.NiContributingYears;
            }
            else
            {
                if (NiContributingYearsCalc.IsThisAContributingDate(now, StepDate, monthlySalary, _person, _taxSystem) && NiContributingYears < 35)
                    NiContributingYears++;
                var predictedStatePensionAnnual = statePensionAmountCalculator.Calculate(NiContributingYears);
                PredictedStatePensionAnnual = Convert.ToInt32(predictedStatePensionAnnual.Amount);
            }

            if (StepDate > personStatePensionDate)
            {
                _preTaxStatePensionIncome = PredictedStatePensionAnnual / _monthly;
            }
        }

        private bool PersonHasQuitWork() => _givenRetirementDate.HasValue ? StepDate > _givenRetirementDate : _calcdMinimum;

        public void UpdateGrowth()
        {
            var growth = Math.Max(Pots.Investments * _assumptions.MonthlyGrowthRate, 0m);
            Growth = growth;
            Pots.AssignIncome(growth);
        }

        public void UpdatePrivatePension()
        {
            var privatePensionGrowth = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;

            if (StepDate >= _privatePensionDate && PersonHasQuitWork())
            {
                //must be annualised (using the monthly figure and multiplying by 12 won't work as 12*monthlyRate != annualRate - because the monthly rate assumes compounding!
                _annualPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.AnnualGrowthRate;
                _monthlyPreTaxPrivatePensionIncome = PrivatePensionAmount * _assumptions.MonthlyGrowthRate;
            }
            else
                PrivatePensionAmount += privatePensionGrowth;

            if (!PersonHasQuitWork())
            {
                var monthlySalary = _person.Salary / _monthly;
                var amount = _person.EmployeeContribution.Amount(monthlySalary);
                var money = _person.EmployerContribution.Amount(monthlySalary);
                PrivatePensionAmount += amount + money;
            }
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

        private decimal PreTaxIncome()
        {
            return (_preTaxSalary * 12) + _annualPreTaxPrivatePensionIncome + (_preTaxStatePensionIncome * 12) + _person.RentalPortfolio.RentalIncome().GetIncomeToPayTaxOn();
        }
        
        public void CalculateChildBenefit(IPersonReport personReport)
        {
            var maxIncome = Math.Max(PreTaxIncome(), personReport?.StepReport.CurrentStep.PreTaxIncome() ?? 0);
            ChildBenefit = ChildBenefitCalc.Amount(StepDate, _person.Children, maxIncome);
            Pots.AssignIncome(ChildBenefit);
        }

        public void PayTaxAndBankTheRemainder()
        {
            //Simplification - calculate tax for the whole year then divide it for the month
            //this will not be accurate on years where someone quits work or starts receiving a pension.
            //In that case the fact they work/receive pension for a partial year would mean their real tax bill is less that calculated here
            var incomeTaxCalculator = new IncomeTaxCalculator(_taxSystem);
            var afterTax = incomeTaxCalculator.TaxFor(_preTaxSalary * 12, _annualPreTaxPrivatePensionIncome, _preTaxStatePensionIncome * 12, _person.RentalPortfolio.RentalIncome());
            AfterTaxSalary = afterTax.AfterTaxIncomeFor(IncomeType.Salary) / 12;
            AfterTaxPrivatePensionIncome = _monthlyPreTaxPrivatePensionIncome - (afterTax.TotalTaxFor(IncomeType.PrivatePension) / 12);
            AfterTaxStatePension = afterTax.AfterTaxIncomeFor(IncomeType.StatePension) / 12;
            AfterTaxRentalIncome = afterTax.AfterTaxIncomeFor(IncomeType.RentalIncome) / 12;

            var newIncome = AfterTaxSalary + AfterTaxPrivatePensionIncome + AfterTaxStatePension + AfterTaxRentalIncome;

            Pots.AssignIncome(newIncome);
        }

        public Take25Result CalcTake25()
        {
            var take25Result = new Take25Rule(_assumptions.LifeTimeAllowance).Result(PrivatePensionAmount);
            return take25Result;
        }

        public void Take25(Take25Result take25Result)
        {
            Pots.AssignIncome(take25Result.TaxFreeAmount);
            PrivatePensionAmount = take25Result.NewPensionPot;
        }

        public decimal CalcLtaCharge()
        {
            return LtaChargeRule.Calc(PrivatePensionAmount, _assumptions.LifeTimeAllowance);
        }

        public void PayLtaCharge(decimal ltaCharge)
        {
            PrivatePensionAmount -= ltaCharge;
        }
    }
}