using System;
using System.Collections.Generic;
using System.Linq;

namespace Pet.Jira.Application.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> WhereIfNotNull<T>(this IEnumerable<T> enumerable,
            Func<T, bool> condition)
        {
            if (condition != null)
            {
                enumerable = enumerable.Where(condition);
            }

            return enumerable;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}
