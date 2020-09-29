using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Calculator.Input;
using Calculator.Output;

namespace Calculator.ExternalInterface
{
    /// <summary>
    /// Generates a retirement report detailing when a user can retire
    /// </summary>
    public interface IRetirementCalculator
    {
        Task<IRetirementReport> ReportForTargetAgeAsync(IEnumerable<Person> personStatus, IEnumerable<SpendingStep> spendingStepInputs, int? retirementAge = null);
    }
}