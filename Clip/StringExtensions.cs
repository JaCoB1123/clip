using System.Text;

namespace Clip
{
    public static class StringExtensions
    {
        public static string RemoveNewlines(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var sb = new StringBuilder(value);
            for (var i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '\r' || sb[i] == '\n')
                    sb.Remove(i--, 1);
            }

            return sb.ToString();
        }
    }
}