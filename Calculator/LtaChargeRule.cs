using System;

namespace Calculator
{
    public static class LtaChargeRule
    {
        public static decimal Calc(in decimal privatePensionAmount, in int lifeTimeAllowance)
        {
            var amountSubjectToLta = Math.Max(0, privatePensionAmount - lifeTimeAllowance);
            var ltaCharge = amountSubjectToLta * .25m;
            return ltaCharge;
        }
    }
}