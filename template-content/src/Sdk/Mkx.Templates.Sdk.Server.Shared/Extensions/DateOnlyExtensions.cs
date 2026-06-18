using System.Globalization;

namespace Mkx.Templates.Sdk.Server.Shared.Extensions;

public static class DateOnlyExtensions
{
    extension(DateOnly date)
    {
        public int GetYear(CultureInfo culture)
        {
            return date.GetYear(culture.Calendar);
        }

        public int GetYear(Calendar calendar)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            return dateTime.GetYear(calendar);
        }

        public int GetMonth(CultureInfo culture)
        {
            return date.GetMonth(culture.Calendar);
        }

        public int GetMonth(Calendar calendar)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            return dateTime.GetMonth(calendar);
        }

        public int GetDay(CultureInfo culture)
        {
            return date.GetDay(culture.Calendar);
        }

        public int GetDay(Calendar calendar)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            return dateTime.GetDay(calendar);
        }

        public DateOnly SetYear(int year)
        {
            return date.SetYear(year, CultureInfo.CurrentCulture);
        }

        public DateOnly SetYear(int year, CultureInfo culture)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            return DateOnly.FromDateTime(dateTime.SetYear(year, culture));
        }

        public DateOnly SetMonth(int month, CultureInfo culture)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            return DateOnly.FromDateTime(dateTime.SetMonth(month, culture));
        }

        public DateOnly AddMonths(int months, CultureInfo culture)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);
            return DateOnly.FromDateTime(dateTime.AddMonths(months, culture));
        }

        public DateOnly GetMonthStart()
        {
            return date.GetMonthStart(CultureInfo.CurrentCulture);
        }

        public DateOnly GetMonthStart(CultureInfo culture)
        {
            return date.GetMonthStart(culture.Calendar);
        }

        public DateOnly GetMonthStart(Calendar calendar)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            return DateOnly.FromDateTime(dateTime.GetMonthStart(calendar));
        }

        public DateOnly GetMonthEnd()
        {
            return date.GetMonthEnd(CultureInfo.CurrentCulture);
        }

        public DateOnly GetMonthEnd(CultureInfo culture)
        {
            return date.GetMonthEnd(culture.Calendar);
        }

        public DateOnly GetMonthEnd(Calendar calendar)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            return DateOnly.FromDateTime(dateTime.GetMonthEnd(calendar));
        }

        public int CalculateAge()
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);
            return dateTime.CalculateAge();
        }
    }
}
