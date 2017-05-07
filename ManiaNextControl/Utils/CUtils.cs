using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNextControl.Utils
{
    public static class CUtils
    {
        public static string ToCommaList(object[] list)
        {
            string addItem(string s, int i)
            {
                return s + (i == 1 ? (", ") : "");
            }

            var returned = "";
            int index = 0;
            foreach (var l in list)
            {
                returned += addItem(l.ToString(), index);
                index++;
            }
            return returned;
        }

        public static T TypeCast<T>(this object o) => (T)o;

        public static List<T> CreateListFromEach<T>(this List<T> hash, Func<object, T> func)
        {
            var list = new List<T>();
            for (int i = 0; i < hash.Count; i++)
                list.Add(func(hash[i]));
            return list;
        }
    }
}
