using System;

namespace BaseClocks
{

    public static partial class DigitalClockUtility
    {
        private const int k_TimeCacheLength = 1440;

        [System.Obsolete]
        public static int EncodeMinuteAndHour(int minute, int hour)
        {
            return hour * 60 + minute;
        }

        public static TimeSpan TwentyFourHourToTwelveHourFormat(TimeSpan timeSpan, out bool isMorning)
        {
            int hour = timeSpan.Hours;

            isMorning = hour < 12;

            hour %= 12;
            if (hour == 0)
            {
                hour += 12;
            }
            return new TimeSpan(hour, timeSpan.Minutes, timeSpan.Seconds);
        }

        [System.Obsolete]
        public static string EncodeMinHourToString(int encoded)
        {
            //if (encoded >= 0 && encoded < k_TimeCacheLength)
            //{
            //    return s_TimeCache[encoded];
            //}
            return "Ob:so";
        }

        public static string ToDisplayString(this DigitalClockFormat format)
        {
            switch (format)
            {
                case DigitalClockFormat.TWELVE_HOUR:
                    return "12-hour (01:00pm)";
                case DigitalClockFormat.TWENTY_FOUR_HOUR:
                    return "24-hour (13:00)";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
