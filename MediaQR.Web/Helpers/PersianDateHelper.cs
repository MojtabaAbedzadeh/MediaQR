using System;
using System.Globalization;

namespace MediaQR.Web.Helpers
{
    public static class PersianDateHelper
    {
        private static readonly PersianCalendar Calendar =
            new PersianCalendar();

        public static string ToPersianDate(DateTime? date)
        {
            if (!date.HasValue)
                return "-";

            var value = date.Value;

            return string.Format(
                "{0:0000}/{1:00}/{2:00}",
                Calendar.GetYear(value),
                Calendar.GetMonth(value),
                Calendar.GetDayOfMonth(value)
            );
        }

        public static string ToPersianDateTime(DateTime? date)
        {
            if (!date.HasValue)
                return "-";

            var value = date.Value;

            return string.Format(
                "{0:0000}/{1:00}/{2:00} {3:00}:{4:00}",
                Calendar.GetYear(value),
                Calendar.GetMonth(value),
                Calendar.GetDayOfMonth(value),
                Calendar.GetHour(value),
                Calendar.GetMinute(value)
            );
        }

        public static string ToPersianDateTimeWithSeconds(DateTime? date)
        {
            if (!date.HasValue)
                return "-";

            var value = date.Value;

            return string.Format(
                "{0:0000}/{1:00}/{2:00} {3:00}:{4:00}:{5:00}",
                Calendar.GetYear(value),
                Calendar.GetMonth(value),
                Calendar.GetDayOfMonth(value),
                Calendar.GetHour(value),
                Calendar.GetMinute(value),
                Calendar.GetSecond(value)
            );
        }
    }
}