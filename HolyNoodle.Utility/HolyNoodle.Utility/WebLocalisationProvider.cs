using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HolyNoodle.Utility
{
    public class WebLocalisationProvider : ILocalisationProvider
    {
        public CultureInfo GetLanguage()
        {
            if (System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session["holynoodle:LocalisationLanguage"] != null)
            {
                return (CultureInfo)System.Web.HttpContext.Current.Session["holynoodle:LocalisationLanguage"];
            }
            if (System.Web.HttpContext.Current.Request != null && System.Web.HttpContext.Current.Request.UserLanguages != null && System.Web.HttpContext.Current.Request.UserLanguages.Any())
            {
                return new CultureInfo(System.Web.HttpContext.Current.Request.UserLanguages[0]);
            }
            return new CultureInfo(LocalisationHelper.DefaultLanguage);
        }

        public void SetLanguage(CultureInfo culture)
        {
            System.Web.HttpContext.Current.Session["holynoodle:LocalisationLanguage"] = culture;
        }
    }
}
