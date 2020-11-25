using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANTLR_Test.Classes
{
    public static class Extensions
    {
        public static string ToFileFormatString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd_HH-mm-ss");
        }
    }
}
