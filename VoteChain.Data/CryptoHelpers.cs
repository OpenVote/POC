using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VoteChain.Data
{
    public class CryptoHelpers
    {
        public static string GenerateRandomKey(int size)
        {
            string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_+=-{[}]\\|:;\",.<>/?";
            char[] chars = new char[size];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            for (var i = 0; i < size; i++)
            {
                var number = GetCryptographicRandomNumber(0, allowedChars.Length - 1, crypto);
                chars[i] = allowedChars[number];
            }
            return new string(chars);

        }

        private static int GetCryptographicRandomNumber(int lBound, int uBound, RNGCryptoServiceProvider rgn)
        {
            // Assumes lBound >= 0 && lBound < uBound
            // returns an int >= lBound and < uBound
            uint urndnum;
            var rndnum = new Byte[4];
            if (lBound == uBound - 1)
            {
                // test for degenerate case where only lBound can be returned
                return lBound;
            }

            uint xcludeRndBase = (uint.MaxValue -
                (uint.MaxValue % (uint)(uBound - lBound)));

            do
            {
                rgn.GetBytes(rndnum);
                urndnum = BitConverter.ToUInt32(rndnum, 0);
            } while (urndnum >= xcludeRndBase);

            return (int)(urndnum % (uBound - lBound)) + lBound;
        }

        public static string ToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }
        public static byte[] FromHexString(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
