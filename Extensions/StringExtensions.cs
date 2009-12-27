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

        public static string ReplaceFirst(this string s, string searchFor, string replaceWith)
        {
            int startIndex = s.IndexOf(searchFor);
            if (startIndex < 0) return s;

            return s.Substring(0, startIndex) + replaceWith + s.Substring(startIndex + searchFor.Length);
        }

        public static int CountOf(this string s, char searchFor)
        {
            int c = 0;
            for (int i = 0; i < s.Length; i++)
                if (s[i] == searchFor)
                    c++;
            return c;
        }
    }
}
