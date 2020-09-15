using System;
using System.Collections.Generic;
using TaxCalculator.Input;
using TaxCalculator.Output;

namespace TaxCalculator.ExternalInterface
{
    /// <summary>
    /// Generates a retirement report detailing when a user can retire
    /// </summary>
    public interface IRetirementCalculator
    {
        IRetirementReport ReportForTargetAge(IEnumerable<Person> personStatus, IEnumerable<SpendingStep> spendingStepInputs, int? retirementAge = null);
    }
}