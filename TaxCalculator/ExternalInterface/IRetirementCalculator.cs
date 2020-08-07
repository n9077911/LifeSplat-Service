using System;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementCalculator
    {
        IRetirementReport ReportForTargetAge(PersonStatus personStatus, int? retirementAge = null);
        IRetirementReport ReportFor(PersonStatus personStatus, DateTime? givenRetirementDate = null);
    }
}