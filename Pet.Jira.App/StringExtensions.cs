using System;

namespace Pet.Jira.App
{
    public static class StringExtensions
    {
        public static String ToFixedString(this String value, int length, char appendChar = ' ')
        {
            return value.PadRight(length, appendChar);
        }
    }
}
