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
        public static int MaxPage<T>(this IEnumerable<T> enumerable, int pageSize)
        {
            if (enumerable.IsNullOrEmpty()) return 0;

            return (int)Math.Ceiling((double)enumerable.Count() / pageSize);
        }


        public static string ToDateTimeString(this DateTime datetime)
        {
            return datetime.ToString("yyyy/MM/dd HH:mm:ss");

        }

       
    }
}