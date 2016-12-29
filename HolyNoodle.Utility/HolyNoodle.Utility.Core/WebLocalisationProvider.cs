using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace HolyNoodle.Utility
{
    public class WebLocalisationProvider : ILocalisationProvider
    {
        public CultureInfo GetLanguage(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor.HttpContext.Session.Keys.Contains("holynoodle:LocalisationLanguage"))
            {
                CultureInfo value = null;
                if((CultureInfo)httpContextAccessor.HttpContext.Session.TryGetValue("holynoodle:LocalisationLanguage", out value)) {
                    return value;
                }
            }
            return new CultureInfo(LocalisationHelper.DefaultLanguage);
        }

        public void SetLanguage(IHttpContextAccessor httpContextAccessor, CultureInfo culture)
        {
            httpContextAccessor.HttpContext.Session.Set("holynoodle:LocalisationLanguage", culture);
        }
    }
}
