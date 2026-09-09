using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins_CommonLibrary.Extensions
{
	public static class DateTimeExtensions
	{
		public static int FiscalYear( this DateTime dt )
		{
			// The start of Fiscal year is Oct 1. 
			if (dt.Month >= 10)
			{
				return dt.Year + 1;
			}
			return dt.Year;
		}

		public static int FiscalYearWithoutCentury( this DateTime dt )
		{
			return (dt.FiscalYear() % 100);
		}

		public static int MonthOfFiscalYear( this DateTime dt )
		{
			// The fiscal year starts in Oct 1 of the previous year
			int fiscalMonth = dt.Month;
			if (fiscalMonth >= 10)
				fiscalMonth -= 9;
			else
				fiscalMonth += 3;

			return fiscalMonth;
		}

		public static int DayOfFiscalYear( this DateTime dt )
		{
			int fiscalYr = dt.FiscalYear();

			// The fiscal year starts in Oct 1 of the previous year
			DateTime fYrStart = new DateTime(fiscalYr - 1, 10, 1);

			// The number of days in the year is the difference between the current date and
			// the start date plus "1" day such that Oct 1 is day "1" not "0".
			return (dt - fYrStart).Days + 1;
		}

		public static int FYTotalDays( this DateTime dt )
		{
			return (DateTime.IsLeapYear(dt.FiscalYear()) ? 366 : 365);
		}

		private const int MinutesInDay = 1440;

		public static DateTime NextBusinessDay(this DateTime dateTime)
		{
			int daysToAdd = (dateTime.DayOfWeek == DayOfWeek.Friday) ? 3 : dateTime.DayOfWeek == DayOfWeek.Saturday ? 2 : 1;

			return dateTime.AddDays(daysToAdd).Date;
		}

		public static bool IsBetween(this DateTime input, DateTime date1, DateTime date2)
		{
			return input >= date1 && input <= date2;
		}

		public static int GetBusinessDays(this DateTime firstDay, DateTime lastDay, params DateTime[] holidays)
		{
			firstDay = firstDay.Date;
			lastDay = lastDay.Date;
			if (firstDay > lastDay)
				throw new ArgumentException("Incorrect last day " + lastDay);

			TimeSpan span = lastDay - firstDay;
			int businessDays = span.Days + 1;
			int fullWeekCount = businessDays / 7;
			// find out if there are weekends during the time exceedng the full weeks
			if (businessDays > fullWeekCount * 7)
			{
				// we are here to find out if there is a 1-day or 2-days weekend
				// in the time interval remaining after subtracting the complete weeks
				int firstDayOfWeek = (int)firstDay.DayOfWeek;
				int lastDayOfWeek = (int)lastDay.DayOfWeek;
				if (lastDayOfWeek < firstDayOfWeek)
					lastDayOfWeek += 7;
				if (firstDayOfWeek <= 6)
				{
					if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
						businessDays -= 2;
					else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
						businessDays -= 1;
				}
				else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
					businessDays -= 1;
			}

			// subtract the weekends during the full weeks in the interval
			businessDays -= fullWeekCount + fullWeekCount;

			// subtract the number of bank holidays during the time interval
			foreach (DateTime bankHoliday in holidays)
			{
				DateTime bh = bankHoliday.Date;
				if (firstDay <= bh && bh <= lastDay)
					--businessDays;
			}

			return businessDays;
		}
	}
}