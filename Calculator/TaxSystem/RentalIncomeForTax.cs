using System;

namespace Calculator.TaxSystem
{
    public class RentalIncomeForTax
    {
        private readonly decimal _grossIncome;
        private readonly decimal _expenses;
        private readonly decimal _financingCosts;
        private readonly int _allowance = 1000;

        public RentalIncomeForTax(decimal grossIncome, decimal expenses, decimal financingCosts)
        {
            _grossIncome = grossIncome;
            _expenses = expenses;
            _financingCosts = financingCosts;
        }

        public decimal GetIncomeToPayTaxOn()
        {
            if (BetterToApplyPropertyAllowance())
                return Math.Max(0, _grossIncome  - _allowance);
            
            return Math.Max(0, _grossIncome  - _expenses);
        }
        
        public decimal GetNetIncome()
        {
            return Math.Max(0, _grossIncome  - _expenses - _financingCosts);
        }

        public decimal TaxDeductibleFinancingCosts()
        {
            if (BetterToApplyPropertyAllowance())
                return 0;
            return _financingCosts;
        }

        //if you apply the allowance you can't apply any deductions 
        private bool BetterToApplyPropertyAllowance()
        {
            return (_financingCosts * .2m) + _expenses < _allowance;
        }
    }
}