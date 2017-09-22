using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILocalisationService
{
    void Load(string defaultLanguage, string rootLanguageDirectory, string pattern);
    Task<string> GetText(string key);
    Task<IDictionary<string, string>> GetAllTexts();
}