using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace UI.ViewModels
{
    public class CurrentWealthViewModel : Screen
    {
        public decimal PrivatePensionTotal { get; set; }
        public decimal CashSavings { get; set; }
        public decimal IsaInvestments { get; set; }
        public decimal OutsideIsaInvestments { get; set; }
        public decimal PropertyIncome { get; set; }
    }
}
