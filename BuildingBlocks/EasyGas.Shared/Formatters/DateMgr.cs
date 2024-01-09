using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace EasyGas.Shared.Formatters
{
    public class DateMgr
    {
        public static DateTime TimetoEst(DateTime timenow)
        {
            var dto = new DateTimeOffset(timenow);  // will use .Kind to decide the offset
            var converted = dto.ToOffset(TimeSpan.FromHours(5.30));
            return converted.DateTime;
        }
        public static DateTime ConvetToIndiaTime(DateTime utDateTime)
        {
            DateTime IndiaTimeNow = utDateTime;
            DateTime myConvertedDateTime = TimeZoneInfo.ConvertTime(
            IndiaTimeNow, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

            return myConvertedDateTime;
        }
        public static double GetTimeDiffFromMorning(DateTime startDateCurrent)
        {
            int diff = startDateCurrent.Hour * 3600 + startDateCurrent.Minute * 60 + startDateCurrent.Second;
            return diff;
        }


        public static DateTime GetIndiaToday()
        {
            try
            {
                DateTime myConvertedDateTime = TimeZoneInfo.ConvertTime(
                DateTime.Now.Date, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                //myConvertedDateTime = TimetoEst(DateTime.Now);
                return myConvertedDateTime;
            }
            //catch (TimeZoneNotFoundException ex)
            //{
            //    System.Diagnostics.Trace.TraceError("Unable to retrieve the Eastern Standard time zone" + ex.Message + ex.InnerException);
            //    Console.WriteLine("Unable to retrieve the Eastern Standard time zone.");

            //}
            catch (InvalidTimeZoneException ex)
            {
                System.Diagnostics.Trace.TraceError("Unable to retrieve the Eastern Standard time zone" + ex.Message + ex.InnerException);
                Console.WriteLine("Unable to retrieve the Eastern Standard time zone.");

            }
            return DateTime.Today;

        }

        public enum TIMEType
        {
            millisec,
            second,
            minute,
            hour,
            day

        }

        public static double DiffrenceFromCurrTime(TIMEType ttype, DateTime futTime)
        {
            TimeSpan span = futTime.Subtract(GetCurrentIndiaTime());
            double diffRounded = 0;
            switch (ttype)
            {
                case TIMEType.millisec:
                    diffRounded = (double)Math.Round(span.TotalMilliseconds);
                    break;
                case TIMEType.second:
                    diffRounded = (double)Math.Round(span.TotalSeconds);
                    break;
                case TIMEType.minute:
                    diffRounded = (double)Math.Round(span.TotalMinutes);
                    break;
                case TIMEType.hour:
                    diffRounded = (double)Math.Round(span.TotalHours);
                    break;

                case TIMEType.day:
                    diffRounded = (double)(span.Days);
                    break;

                default:
                    break;
            }

            return diffRounded;
        }
        public static double DiffrenceFromStartTime(TIMEType ttype, DateTime startTime, DateTime futTime)
        {
            TimeSpan span = futTime.Subtract(startTime);
            double diffRounded = 0;
            switch (ttype)
            {
                case TIMEType.millisec:
                    diffRounded = (double)Math.Round(span.TotalMilliseconds);
                    break;
                case TIMEType.second:
                    diffRounded = (double)Math.Round(span.TotalSeconds);
                    break;
                case TIMEType.minute:
                    diffRounded = (double)Math.Round(span.TotalMinutes);
                    break;
                case TIMEType.hour:
                    diffRounded = (double)Math.Round(span.TotalHours);
                    break;
                case TIMEType.day:
                    diffRounded = (double)(span.Days);
                    break;
                default:
                    break;
            }

            return diffRounded;
        }

        public static String GetTimeString(int nAdvance)
        {
            String time = "UKnown";
            time = GetCurrentIndiaTime().AddSeconds(nAdvance).ToString();
            return time;
        }

        public static String GetTimeString(DateTime startTime, int nAdvance)
        {
            String time = "UKnown";
            time = startTime.AddSeconds(nAdvance).ToString();
            return time;
        }
        public static DateTime GetCurrentIndiaTime()
        {
            try
            {
                TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo("India Standard Time");
                //TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo("E.Africa Standard Time"); //not working
                DateTime IndiaTimeNow = DateTime.Now;
                DateTime myConvertedDateTime = TimeZoneInfo.ConvertTime(
                IndiaTimeNow, tzi);

                //myConvertedDateTime = myConvertedDateTime.AddMinutes(-150); //convert to kenya time for EasyGas kenya

                return myConvertedDateTime;
            }
            //catch (TimeZoneNotFoundException ex)
            //{
            //    System.Diagnostics.Trace.TraceError("Unable to retrieve the Eastern Standard time zone" + ex.Message + ex.InnerException);
            //    Console.WriteLine("Unable to retrieve the Eastern Standard time zone.");

            //}
            catch (InvalidTimeZoneException ex)
            {
                System.Diagnostics.Trace.TraceError("Unable to retrieve the Eastern Standard time zone" + ex.Message + ex.InnerException);
                Console.WriteLine("Unable to retrieve the Eastern Standard time zone.");
            }
            return DateTime.Now;
        }
    }
}
