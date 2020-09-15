using System.Collections.Generic;
using TaxCalculator.ExternalInterface;

namespace TaxCalculator.TaxSystem
{
    ///The output of an income tax calculation
    public class TaxResult : ITaxResult
    {
        private readonly decimal _totalIncome;
        private readonly Dictionary<IncomeType, decimal> _taxPerIncomeType = new Dictionary<IncomeType, decimal>();
        private readonly Dictionary<IncomeType, decimal> _incomePerIncomeType = new Dictionary<IncomeType, decimal>();

        public TaxResult(decimal totalIncome)
        {
            _totalIncome = totalIncome;
            AfterTaxIncome = _totalIncome;
        }

        public decimal IncomeTax { get; private set; }
        public decimal NationalInsurance { get; private set; }
        public decimal TotalTax { get; private set; }
        public decimal AfterTaxIncome { get; private set; }

        public decimal TotalTaxFor(IncomeType type)
        {
            return _taxPerIncomeType.ContainsKey(type) ? _taxPerIncomeType[type] : 0;
        }

        public decimal AfterTaxIncomeFor(IncomeType type)
        {
            if(_incomePerIncomeType.ContainsKey(type) && _taxPerIncomeType.ContainsKey(type))
                return _incomePerIncomeType[type] - _taxPerIncomeType[type];
            return 0;
        }

        public void AddIncomeTax(decimal incomeTax, IncomeType incomeType)
        {
            _taxPerIncomeType[incomeType] = _taxPerIncomeType[incomeType] + incomeTax;
            
            IncomeTax += incomeTax;
            TotalTax += incomeTax;
            AfterTaxIncome = _totalIncome - TotalTax;
        }

        public void AddNationalInsurance(decimal nationalInsurance, IncomeType incomeType)
        {
            _taxPerIncomeType[incomeType] = _taxPerIncomeType[incomeType] + nationalInsurance;
            NationalInsurance += nationalInsurance;
            TotalTax += nationalInsurance;
            AfterTaxIncome = _totalIncome - TotalTax;
        }

        public void AddIncomeFor(decimal income, IncomeType incomeType)
        {
            _taxPerIncomeType.Add(incomeType, 0);
            _incomePerIncomeType.Add(incomeType, income);            
        }
    }
}