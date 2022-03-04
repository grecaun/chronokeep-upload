using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using System.Windows;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace results_uploader.Objects.Helpers
{
    public class Utils
    {
        private static Application ExcelApp;

        public static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] charArray = s.ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            return new string(charArray);
        }
    }
}
