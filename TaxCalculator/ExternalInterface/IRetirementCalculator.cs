using System;
using System.Collections.Generic;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementCalculator
    {
        IRetirementReport ReportForTargetAge(PersonStatus personStatus, int? retirementAge = null);
        IRetirementReport ReportForTargetAge(IEnumerable<PersonStatus> personStatus, int? retirementAge = null);
    }
}