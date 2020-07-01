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
    class ShellViewModel : Conductor<object>
    {
        public void LoadIncomeTaxPage()
        {
            ActivateItem(new IncomeTaxViewModel());
        }
    }
}
