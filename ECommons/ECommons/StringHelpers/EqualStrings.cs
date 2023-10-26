using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommons.StringHelpers
{
    public static class EqualStrings
    {
        static List<HashSet<string>> Equalities = new();

        public static void RegisterEquality(IEnumerable<string> values)
        {
            Equalities.Add(values.ToHashSet());
        }

        public static void RegisterEquality(params string[] values) => RegisterEquality((IEnumerable<string>)values);

        public static bool Equals(string what, string with, StringComparison comparison = StringComparison.Ordinal)
        {
            if(what.Equals(with, comparison)) return true;
            var equalityList = Equalities.FirstOrDefault(x => x.Any(z => z.Equals(with, comparison)));
            if(equalityList != null)
            {
                return equalityList.Any(x => x.Equals(what, comparison));
            }
            else
            {
                return false;
            }
        }
        public static bool ESEquals(this string what, string with, StringComparison comparison = StringComparison.Ordinal) => Equals(what, with, comparison);

        public static bool EqualsAny(string what, IEnumerable<string> with)
        {
            foreach(var x in with)
            {
                if (Equals(what, x)) return true;
            }
            return false;
        }
        public static bool EqualsAny(string what, params string[] with) => EqualsAny(what, (IEnumerable<string>)with);
        public static bool ESEqualsAny(this string what, IEnumerable<string> with) => EqualsAny(what, with);
        public static bool ESEqualsAny(this string what, params string[] with) => EqualsAny(what, (IEnumerable<string>)with);


        public static bool EqualsAnyIgnoreCase(string what, IEnumerable<string> with)
        {
            foreach (var x in with)
            {
                if (Equals(what, x, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
        public static bool EqualsAnyIgnoreCase(string what, params string[] with) => EqualsAnyIgnoreCase(what, (IEnumerable<string>)with);
        public static bool ESEqualsAnyIgnoreCase(this string what, IEnumerable<string> with) => EqualsAnyIgnoreCase(what, with);
        public static bool ESEqualsAnyIgnoreCase(this string what, params string[] with) => EqualsAnyIgnoreCase(what, (IEnumerable<string>)with);

        internal static void Dispose()
        {
            Equalities = null;
        }
    }
}
