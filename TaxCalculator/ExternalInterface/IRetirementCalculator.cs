using System;
using System.Collections.Generic;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementCalculator
    {
        IRetirementReport ReportForTargetAge(IEnumerable<PersonStatus> personStatus, IEnumerable<SpendingStepInput> spendingStepInputs, int? retirementAge = null);
    }
}