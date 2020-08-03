using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TaxCalculator
{
    //info taken from https://www.gov.uk/government/publications/state-pension-age-timetable/state-pension-age-timetable
    public class PensionAgeCalc : IPensionAgeCalc
    {
        private static readonly List<(string, string)> MensPensionAgeTable = new List<(string, string)>
        {
            ("1 January 1900 - 5 December 1953", "65-0"),
        };
        
        private static readonly List<(string, string)> WomensPensionAgeTable = new List<(string, string)>
        {
            ("1 January 1900 - 5 April 1950", "60-0"),
            ("6 April 1950 - 5 May 1950", "6 May 2010"),
            ("6 May 1950 - 5 June 1950", "6 July 2010"),
            ("6 June 1950 - 5 July 1950", "6 September 2010"),
            ("6 July 1950 - 5 August 1950", "6 November 2010"),
            ("6 August 1950 - 5 September 1950", "6 January 2011"),
            ("6 September 1950 - 5 October 1950", "6 March 2011"),
            ("6 October 1950 - 5 November 1950", "6 May 2011"),
            ("6 November 1950 - 5 December 1950", "6 July 2011"),
            ("6 December 1950 - 5 January 1951", "6 September 2011"),
            ("6 January 1951 - 5 February 1951", "6 November 2011"),
            ("6 February 1951 - 5 March 1951", "6 January 2012"),
            ("6 March 1951 - 5 April 1951", "6 March 2012"),
            ("6 April 1951 - 5 May 1951", "6 May 2012"),
            ("6 May 1951 - 5 June 1951", "6 July 2012"),
            ("6 June 1951 - 5 July 1951", "6 September 2012"),
            ("6 July 1951 - 5 August 1951", "6 November 2012"),
            ("6 August 1951 - 5 September 1951", "6 January 2013"),
            ("6 September 1951 - 5 October 1951", "6 March 2013"),
            ("6 October 1951 - 5 November 1951", "6 May 2013"),
            ("6 November 1951 - 5 December 1951", "6 July 2013"),
            ("6 December 1951 - 5 January 1952", "6 September 2013"),
            ("6 January 1952 - 5 February 1952", "6 November 2013"),
            ("6 February 1952 - 5 March 1952", "6 January 2014"),
            ("6 March 1952 - 5 April 1952", "6 March 2014"),
            ("6 April 1952 - 5 May 1952", "6 May 2014"),
            ("6 May 1952 - 5 June 1952", "6 July 2014"),
            ("6 June 1952 - 5 July 1952", "6 September 2014"),
            ("6 July 1952 - 5 August 1952", "6 November 2014"),
            ("6 August 1952 - 5 September 1952", "6 January 2015"),
            ("6 September 1952 - 5 October 1952", "6 March 2015"),
            ("6 October 1952 - 5 November 1952", "6 May 2015"),
            ("6 November 1952 - 5 December 1952", "6 July 2015"),
            ("6 December 1952 - 5 January 1953", "6 September 2015"),
            ("6 January 1953 - 5 February 1953", "6 November 2015"),
            ("6 February 1953 - 5 March 1953", "6 January 2016"),
            ("6 March 1953 - 5 April 1953", "6 March 2016"),
            ("6 April 1953 - 5 May 1953", "6 July 2016"),
            ("6 May 1953 - 5 June 1953", "6 November 2016"),
            ("6 June 1953 - 5 July 1953", "6 March 2017"),
            ("6 July 1953 - 5 August 1953", "6 July 2017"),
            ("6 August 1953 - 5 September 1953", "6 November 2017"),
            ("6 September 1953 - 5 October 1953", "6 March 2018"),
            ("6 October 1953 - 5 November 1953", "6 July 2018"),
            ("6 November 1953 - 5 December 1953", "6 November 2018"),
        };
        
        private static readonly List<(string, string)> PensionAgeTable = new List<(string, string)>()
        {
            ("6 December 1953 - 5 January 1954", "6 March 2019"),
            ("6 January 1954 - 5 February 1954", "6 May 2019"),
            ("6 February 1954 - 5 March 1954", "6 July 2019"),
            ("6 March 1954 - 5 April 1954", "6 September 2019"),
            ("6 April 1954 - 5 May 1954", "6 November 2019"),
            ("6 May 1954 - 5 June 1954", "6 January 2020"),
            ("6 June 1954 - 5 July 1954", "6 March 2020"),
            ("6 July 1954 - 5 August 1954", "6 May 2020"),
            ("6 August 1954 - 5 September 1954", "6 July 2020"),
            ("6 September 1954 - 5 October 1954", "6 September 2020"),
            ("6 October 1954 - 5 April 1960", "66-0"),
            ("6 April 1960 - 5 May 1960", "66-1"),
            ("6 May 1960 - 5 June 1960", "66-2"),
            ("6 June 1960 - 5 July 1960", "66-3"),
            ("6 July 1960 - 5 August 1960", "66-4"),
            ("6 August 1960 - 5 September 1960", "66-5"),
            ("6 September 1960 - 5 October 1960", "66-6"),
            ("6 October 1960 - 5 November 1960", "66-7"),
            ("6 November 1960 - 5 December 1960", "66-8"),
            ("6 December 1960 - 5 January 1961", "66-9"),
            ("6 January 1961 - 5 February 1961", "66-10"),
            ("6 February 1961 - 5 March 1961", "66-11"),
            ("6 March 1961 - 5 April 1977",	"67-0"),
            ("6 April 1977 - 5 May 1977", "6 May 2044"),
            ("6 May 1977 - 5 June 1977", "6 July 2044"),
            ("6 June 1977 - 5 July 1977", "6 September 2044"),
            ("6 July 1977 - 5 August 1977", "6 November 2044"),
            ("6 August 1977 - 5 September 1977", "6 January 2045"),
            ("6 September 1977 - 5 October 1977", "6 March 2045"),
            ("6 October 1977 - 5 November 1977", "6 May 2045"),
            ("6 November 1977 - 5 December 1977", "6 July 2045"),
            ("6 December 1977 - 5 January 1978", "6 September 2045"),
            ("6 January 1978 - 5 February 1978", "6 November 2045"),
            ("6 February 1978 - 5 March 1978", "6 January 2046"),
            ("6 March 1978 - 5 April 1978", "6 March 2046"),
            ("6 April 1978 - 1 January 2099", "68-0"),
        };

        private static List<(DateTime, DateTime, IRelativeDateProvider)> _mensLookupTable;
        private static List<(DateTime, DateTime, IRelativeDateProvider)> _womensLookupTable;
        private static List<(DateTime, DateTime, IRelativeDateProvider)> _jointLookupTable;

        public DateTime StatePensionDate(DateTime dob, Sex sex)
        {
            PrepareLookupTables();

            var dateRange = FindDateRange(dob, sex == Sex.Male ? _mensLookupTable : _womensLookupTable);

            dateRange = dateRange ?? FindDateRange(dob, _jointLookupTable);

            if (dateRange == null) 
                throw new Exception($"Unknown state retirement date for {dob:d}");
            
            return dateRange.DateFor(dob);
        }

        private static IRelativeDateProvider FindDateRange(DateTime dob, List<(DateTime, DateTime, IRelativeDateProvider)> lookupTable)
        {
            foreach (var dateRange in lookupTable)
            {
                if (dateRange.Item1 <= dob && dateRange.Item2 >= dob)
                    return dateRange.Item3;
            }

            return null;
        }

        public DateTime PrivatePensionDate(DateTime statePensionDate)
        {
            //There is a legal question mark as to whether this blanket rule applies.
            //For those with a state pension age of 68 its not been confirmed that the private pension age is 58.
            return statePensionDate.AddYears(-10);
        }

        private static void PrepareLookupTables()
        {
            if (_jointLookupTable == null)
            {
                _mensLookupTable = new List<(DateTime, DateTime, IRelativeDateProvider)>();
                _womensLookupTable = new List<(DateTime, DateTime, IRelativeDateProvider)>();
                _jointLookupTable = new List<(DateTime, DateTime, IRelativeDateProvider)>();
                PrepareLookupTable(MensPensionAgeTable, _mensLookupTable);
                PrepareLookupTable(WomensPensionAgeTable, _womensLookupTable);
                PrepareLookupTable(PensionAgeTable, _jointLookupTable);
            }
        }

        private static void PrepareLookupTable(List<(string, string)> jointTable, List<(DateTime, DateTime, IRelativeDateProvider)> lookUpTable)
        {
            foreach (var tuple in jointTable)
            {
                var dates = tuple.Item1.Split(new[] {" - "}, StringSplitOptions.None);
                var start = DateTime.ParseExact(dates[0], "d MMMM yyyy", CultureInfo.CurrentCulture);
                var end = DateTime.ParseExact(dates[1], "d MMMM yyyy", CultureInfo.CurrentCulture);

                var dateProvider = new Regex(@"^\d\d").IsMatch(tuple.Item2)
                    ? (IRelativeDateProvider) new BirthdayDateProvider(
                        int.Parse(tuple.Item2.Split('-')[0]), int.Parse(tuple.Item2.Split('-')[1]))
                    : new Date(DateTime.ParseExact(tuple.Item2, "d MMMM yyyy", CultureInfo.CurrentCulture));

                lookUpTable.Add((start, end, dateProvider));
            }
        }

        private class Date : IRelativeDateProvider
        {
            private readonly DateTime _dateTime;

            public Date(DateTime dateTime)
            {
                _dateTime = dateTime;
            }

            public DateTime DateFor(DateTime originDate)
            {
                return _dateTime;
            }
        }

        private class BirthdayDateProvider : IRelativeDateProvider
        {
            private readonly int _age;
            private readonly int _months;

            public BirthdayDateProvider(int age, int months)
            {
                _age = age;
                _months = months;
            }

            public DateTime DateFor(DateTime originDate)
            {
                return originDate.AddYears(_age).AddMonths(_months);
            }
        }

        private interface IRelativeDateProvider
        {
            DateTime DateFor(DateTime originDate);
        }
    }
}