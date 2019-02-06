using System;

namespace retns.api.Services.Extensions {
    public static class DateTimeExtensions {
        public static DateTime GetNextWeekday(this DateTime dt, DayOfWeek day) {
            int daysToAdd = ((int)day - (int)dt.DayOfWeek + 7) % 7;
            // TODO: Remove the -7 adddays!!!!
            return dt.AddDays(-7).AddDays(daysToAdd);
        }
        public static DateTime GetThisMonday(this DateTime dt, DayOfWeek day = DayOfWeek.Monday) {
            int diff = (7 + (dt.DayOfWeek - day)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}