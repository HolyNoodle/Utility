using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility
{
    public interface ILocalisationProvider
    {
        CultureInfo GetLanguage();
        void SetLanguage(CultureInfo culture);
    }

    public enum ApplicationType
    {
        Web,
        StandAlone
    }
}
