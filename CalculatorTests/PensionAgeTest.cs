using System;
using NUnit.Framework;
using Calculator;
using Calculator.StatePensionCalculator;

namespace CalculatorTests
{
    public class PensionAgeTest
    {
        [Test]
        public void CanCalculateAPensionDate()
        {
            //date range to fixed date rule
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1953, 12, 6), Sex.Male), Is.EqualTo(new DateTime(2019, 3, 6)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1953, 12, 15), Sex.Female), Is.EqualTo(new DateTime(2019, 3, 6)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1954, 1, 5), Sex.Male), Is.EqualTo(new DateTime(2019, 3, 6)));
            
            //Birthday based rule
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1954, 10, 6), Sex.Female), Is.EqualTo(new DateTime(2020, 10, 6)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1958, 7, 15), Sex.Male), Is.EqualTo(new DateTime(2024, 7, 15)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1960, 04, 5), Sex.Female), Is.EqualTo(new DateTime(2026, 4, 5)));
            
            //Men based rule
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1953, 12, 5), Sex.Male), Is.EqualTo(new DateTime(2018, 12, 5)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1953, 12, 6), Sex.Male), Is.EqualTo(new DateTime(2019, 3, 6)));
            
            //Women based rule
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1950, 4, 5), Sex.Female), Is.EqualTo(new DateTime(2010, 4, 5)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1950, 4, 6), Sex.Female), Is.EqualTo(new DateTime(2010, 5, 6)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1951, 1, 12), Sex.Female), Is.EqualTo(new DateTime(2011, 11, 6)));
            Assert.That(new PensionAgeCalc().StatePensionDate(new DateTime(1953, 12, 05), Sex.Female), Is.EqualTo(new DateTime(2018, 11, 6)));

            //born today
            var now = DateTime.Now;
            Assert.That(new PensionAgeCalc().StatePensionDate(now, Sex.Female), Is.EqualTo(now.AddYears(68)));
            Assert.That(new PensionAgeCalc().StatePensionDate(now, Sex.Male), Is.EqualTo(now.AddYears(68)));
            
            //born 100 years ago
            Assert.That(new PensionAgeCalc().StatePensionDate(now.AddYears(-100), Sex.Female), Is.EqualTo(now.AddYears(-40)));
            Assert.That(new PensionAgeCalc().StatePensionDate(now.AddYears(-100), Sex.Male), Is.EqualTo(now.AddYears(-35)));
            
            //not born
            Assert.That(new PensionAgeCalc().StatePensionDate(now.AddYears(1), Sex.Female), Is.EqualTo(now.AddYears(69)));
            Assert.That(new PensionAgeCalc().StatePensionDate(now.AddYears(1), Sex.Male), Is.EqualTo(now.AddYears(69)));
        }
    }
}