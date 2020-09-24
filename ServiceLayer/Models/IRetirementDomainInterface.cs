﻿using System;
using System.Collections.Generic;
using Calculator.ExternalInterface;
using ServiceLayer.Models.DTO;

namespace ServiceLayer.Models
{
    public interface IRetirementDomainInterface
    {
        RetirementReportDto RetirementReportFor(int? targetRetirementAge, string emergencyFund, IEnumerable<SpendingStepInputDto> spendingSteps, IEnumerable<PersonDto> persons);
    }
}