using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using TaxCalculator;
using UI.Models;

namespace UI.ViewModels
{
    public class IncomeTaxViewModel : Screen
    {
        private string _paye = "50000";
        private decimal _incomeTax;

        public string Paye
        {
            get => _paye;
            set
            {
                _paye = value;
                NotifyOfPropertyChange(nameof(Paye));
            }
        }

        public decimal IncomeTax
        {
            get => _incomeTax;
            set
            {
                _incomeTax = value;
                NotifyOfPropertyChange(nameof(IncomeTax));
            }
        }

        public ObservableCollection<TaxDescription> TaxDescriptions { get; } = new ObservableCollection<TaxDescription>();

        public void Calc()
        {
            TaxDescriptions.Clear();
            var salary = Int32.Parse(_paye);
            var taxResult = IncomeTaxCalculator.TaxFor(salary);
            TaxDescriptions.Add(new TaxDescription { Description = "Income Tax:", Amount = taxResult.IncomeTax });
            TaxDescriptions.Add(new TaxDescription { Description = "N.I.:", Amount = taxResult.NationalInsurance });
            TaxDescriptions.Add(new TaxDescription { Description = "Take Home:", Amount = salary - taxResult.IncomeTax - taxResult.NationalInsurance });
            IncomeTax = taxResult.IncomeTax;
        }
    }
}
