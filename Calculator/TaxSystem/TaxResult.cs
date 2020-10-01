using System.Collections.Generic;
using System.Linq;

namespace Calculator.TaxSystem
{
    ///The output of an income tax calculation
    public class TaxResult : ITaxResult
    {
        private readonly Dictionary<IncomeType, decimal> _taxPerIncomeType = new Dictionary<IncomeType, decimal>();
        private readonly Dictionary<IncomeType, decimal> _incomePerIncomeType = new Dictionary<IncomeType, decimal>();
        private decimal _rentalTaxCredit;
        private decimal _totalTax;
        private decimal _incomeTax;

        public decimal IncomeTax => _incomeTax - _rentalTaxCredit;

        public decimal NationalInsurance { get; private set; }

        public decimal TotalTax => _totalTax - _rentalTaxCredit;

        public decimal AfterTaxIncome => _incomePerIncomeType.Select(pair => pair.Value).Sum() -TotalTax;

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
            
            _incomeTax += incomeTax;
            _totalTax += incomeTax;
        }

        public void AddNationalInsurance(decimal nationalInsurance, IncomeType incomeType)
        {
            _taxPerIncomeType[incomeType] = _taxPerIncomeType[incomeType] + nationalInsurance;
            NationalInsurance += nationalInsurance;
            _totalTax += nationalInsurance;
        }

        public void AddIncomeFor(decimal income, IncomeType incomeType)
        {
            if(!_taxPerIncomeType.ContainsKey(incomeType))
                _taxPerIncomeType.Add(incomeType, 0);

            if(!_incomePerIncomeType.ContainsKey(incomeType))
                _incomePerIncomeType.Add(incomeType, 0);

            _incomePerIncomeType[incomeType] += income;
        }

        public void AddRentalTaxCredit(decimal taxCredit)
        {
            _rentalTaxCredit += taxCredit;
        }
    }
}