using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility
{
    public static class TraceHelper
    {
        const string _messageTemplate = "{0} : {1}";
        const string _dateFormat = "yyyy-MM-dd hh:mm:ss.fff";

        private static string FormatDate()
        {
            return DateTime.Now.ToString(_dateFormat);
        }

        public static void Error(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(string.Format(_messageTemplate, FormatDate(), ex.Message + ex.StackTrace));
            var inner = ex.InnerException;
            while(inner != null)
            {
                Error(inner);
                inner = inner.InnerException;
            }
        }

        public static void Warning(string message)
        {
            System.Diagnostics.Trace.TraceWarning(string.Format(_messageTemplate, FormatDate(), message));
        }

        public static void Information(string message)
        {
            System.Diagnostics.Trace.TraceInformation(string.Format(_messageTemplate, FormatDate(), message));
        }

        public static void TraceAppHealth()
        {
            System.Diagnostics.Trace.TraceInformation(string.Format(_messageTemplate, FormatDate(), "Trace app health"));
        }
    }
}
