using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.Utils
{
    public static class Helper
    {
        public static TResult GetPropertyValue<TResult>(this object t, string propertyName)
        {
            object val = t.GetType().GetProperties().Single(pi => pi.Name == propertyName).GetValue(t, null);
            return (TResult)val;
        }

        public static void SetPropertyValue<TResult>(this object t, string propertyName, TResult value)
        {
            t.GetType().GetProperties().Single(pi => pi.Name == propertyName).SetValue(t, value, null);
        }
    }
}
