using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HolyNoodle.Utility
{
    public class LocalisationProvider : ILocalisationProvider
    {
        public CultureInfo GetLanguage()
        {
            return Thread.CurrentThread.CurrentUICulture;
        }

        public void SetLanguage(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
