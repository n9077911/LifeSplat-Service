using System;
using System.Collections.Generic;
using System.Linq;

namespace Calculator
{
    public static class ChildBenefitCalc
    {
        public static decimal Amount(DateTime now, IEnumerable<DateTime> childrenDob, decimal maxIncome)
        {                
            var yearsAgo18 = now.AddYears(-18);

            var kids = childrenDob.Count(dob => dob > yearsAgo18);
            var firstKid = kids > 0 ? 1 : 0;
            var otherKids = Math.Max(0, kids - 1);

            var total = ((firstKid*21.05m) + (otherKids*13.95m)) * 52;
            
            var clawBackPercent = Math.Max(0m, Math.Min(100, (maxIncome - 50_000) / 100));
            var keepPercent = (100 - Math.Floor(clawBackPercent))/100;
            
            return (total/12) * keepPercent;
        }
    }
}