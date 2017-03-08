using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.Services
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return true;

            var collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                return collection.Count < 1;
            }

            return !collection.Any();

        }

       
    }
}