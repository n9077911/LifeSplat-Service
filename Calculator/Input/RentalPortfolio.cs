using System;
using System.Collections.Generic;
using System.Linq;
using Calculator.TaxSystem;

namespace Calculator.Input
{
    public class RentalPortfolio
    {
        private readonly List<RentalInfo> _rentalInfos;

        public RentalPortfolio(List<RentalInfo> rentalInfos)
        {
            _rentalInfos = rentalInfos;
        }

        private decimal TotalIncomeAfterExpenses()
        {
            var grossIncome = GrossIncome();
            var totalExpenses = Expenses();
            return grossIncome - totalExpenses;
        }

        private int Expenses()
        {
            return _rentalInfos.Select(info => (int)info.Expenses).Sum();
        }

        private int GrossIncome()
        {
            return _rentalInfos.Select(info => (int)info.GrossIncome).Sum();
        }

        public decimal TotalNetIncome()
        {
            return TotalIncomeAfterExpenses() - FinancingCosts();
        }

        private decimal FinancingCosts()
        {
            if (_rentalInfos.Any(rentalInfo => rentalInfo.Repayment))
                throw new Exception("Repayment mortgages not yet supported");

            return _rentalInfos.Select(info => info.MortgagePayments).Sum();
        }

        public RentalIncomeForTax RentalIncome()
        {
            return new RentalIncomeForTax(GrossIncome(), Expenses(), FinancingCosts());
        }
    }
}