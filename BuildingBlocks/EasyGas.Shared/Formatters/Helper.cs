using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EasyGas.Shared.Formatters
{
    public class Helper
    {
        public static string HashPassword(string password)
        {
            var provider = MD5.Create();
            string salt = "e9m3fujnfs08R@nd0mSalt";
            byte[] bytes = provider.ComputeHash(Encoding.ASCII.GetBytes(salt + password));
            string computedHash = BitConverter.ToString(bytes);
            return computedHash;
        }

        public static Source GetSourceFromHeader(string clientAppNameHeaderValue)
        {
            if (!string.IsNullOrEmpty(clientAppNameHeaderValue))
            {
                if (!Enum.TryParse(clientAppNameHeaderValue, out Source requestSource))
                {
                    requestSource = Source.CUSTOMER_APP;
                }
                return requestSource;
            }
            return Source.CUSTOMER_APP;
        }

        public static double DistanceBetweenPlacesinMtr(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // km

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));
            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;
            double d = Math.Acos(cosD);
            double dist = R * d * 1000;
            //dist = 1;
            dist += dist * 30 / 100; //it is road distance so I am adding 30%

            /*
              var R = 6371; // Radius of the earth in km
                var dLat = deg2rad(lat2 - lat1);  // deg2rad below
                var dLon = deg2rad(lng2 - lng1);
                var a =
                  Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                  Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) *
                  Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                  ;
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var d = R * c; // Distance in km
                */
            return dist > 0 ? dist : 0;
        }

        public static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        // number plate logic
        public static string GenerateNextCodeByAlphaNumSequence(string lastOrderCode = null)
        {
            string nextCode = "";
            if (string.IsNullOrEmpty(lastOrderCode))
            {
                nextCode = "A0001";
            }
            else
            {
                var alpha = lastOrderCode.Substring(0, lastOrderCode.Length - 4);
                var last4DigitStr = lastOrderCode.Substring(lastOrderCode.Length - 4);
                int last4Digit = int.Parse(last4DigitStr);
                if (last4Digit < 9999)
                {
                    last4Digit += 1;
                }
                else
                {
                    last4Digit = 0;
                    alpha = Helper.GetNextAlphaCode(alpha);
                }
                nextCode = alpha + last4Digit.ToString().PadLeft(4, '0');
            }

            return nextCode;
        }

        private static string GetNextAlphaCode(string alpha)
        {
            string nextAlpha = "A";
            char lastChar = alpha.Last();
            string remStr = alpha.Substring(0, alpha.Length - 1);

            if (lastChar == 'Z')
            {
                if (alpha.Length == 1)
                {
                    nextAlpha = "AA";
                }
                else
                {
                    nextAlpha = GetNextAlphaCode(remStr) + nextAlpha;
                }

            }
            else
            {
                nextAlpha = remStr + (char)(((int)lastChar) + 1);
            }

            return nextAlpha;
        }

        public static string GenerateRandomAlphaNumericString(int length)
        {
            string[] _alpaNumericCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            string randomString = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                sTempChars = _alpaNumericCharacters[rand.Next(0, _alpaNumericCharacters.Length)];
                randomString += sTempChars.ToLower();
            }
            return randomString;
        }

        public static string GenerateRandomNumericString(int length, string? defaultStr = null)
        {
            if (!string.IsNullOrEmpty(defaultStr))
            {
                return defaultStr;
            }

            string[] saAllowedCharacters = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            string sOTP = String.Empty;
            string sTempChars = String.Empty;
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                int p = rand.Next(0, saAllowedCharacters.Length);
                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];
                sOTP += sTempChars;
            }
            return sOTP;
        }
    }
}
