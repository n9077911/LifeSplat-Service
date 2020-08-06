using System;

namespace TaxCalculator.ExternalInterface
{
    public interface IRetirementCalculator
    {
        IRetirementReport ReportFor(PersonStatus personStatus, int? retirementAge = null);
    }
}