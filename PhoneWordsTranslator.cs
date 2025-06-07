using System.Text;

namespace MauiApp1
{
    public static class PhoneWordsTranslator
    {
        public static string ToNumber(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            raw = raw.ToUpperInvariant();

            var newRaw = new StringBuilder();
            foreach (char c in raw)
            {
                if ("-0123456789".Contains(c))
                    newRaw.Append(c);
                else
                {
                    var result = TranslateToNumber(c);
                    if (result != null)
                        newRaw.Append(result);
                    else
                        return null;
                }
            }
            return newRaw.ToString();
        }
        static bool Contains(this string keyString, char c)
        {
            return keyString.IndexOf(c) >= 0;
        }

        static readonly string[] digits =
        {
            "ABC", "DEF", "GHI", "JKL", "MNO", "PQRS", "TUV", "WXYZ"
        };
        static int? TranslateToNumber(char c)
        {
            for (int i = 0; i < digits.Length; i++)
            {
                if (digits[i].Contains(c))
                    return 2 + i;
            }
            return null;
        }
    }
}
