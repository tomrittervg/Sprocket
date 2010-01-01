using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sprocket
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <param name="s">The string you are operating on</param>
        /// <param name="needle">The string you want to replace</param>
        /// <param name="replacement">What you want to replace it with</param>
        /// <returns>The same string, if the needle was not found, otherwise the string with the first occurrence of <paramref name="needle"/> replaced with <paramref name="replacement"/>,but no other occurrences.</returns>
        public static string ReplaceFirst(this string s, string needle, string replacement)
        {
            int startIndex = s.IndexOf(needle);
            if (startIndex < 0) return s;

            return s.Substring(0, startIndex) + replacement + s.Substring(startIndex + needle.Length);
        }
        /// <param name="s">The string you are operating on</param>
        /// <param name="needle">The string you want to replace</param>
        /// <param name="replacement">What you want to replace it with</param>
        /// <returns>The same string, if the needle was not found, otherwise the string with the last occurrence of <paramref name="needle"/> replaced with <paramref name="replacement"/>,but no other occurrences.</returns>
        public static string ReplaceLast(this string s, string needle, string replacement)
        {
            int startIndex = s.LastIndexOf(needle);
            if (startIndex < 0) return s;

            return s.Substring(0, startIndex) + replacement + s.Substring(startIndex + needle.Length);
        }

        public static int CountOf(this string s, char searchFor)
        {
            int c = 0;
            for (int i = 0; i < s.Length; i++)
                if (s[i] == searchFor)
                    c++;
            return c;
        }

        public static bool OneOf(this string s, params string[] possibilities)
        {
            for(int i=0;i<possibilities.Length;i++)
                if(s == possibilities[i])
                    return true;
            return false;
        }
    }
}
