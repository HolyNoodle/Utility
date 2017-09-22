using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

public class LocalisationService : ILocalisationService
{
    private Dictionary<string, Dictionary<string, string>> _texts;
    private Dictionary<string, List<FileInfo>> _files;
    
    public static string DefaultLanguage { get; set; }
    internal IRequestCultureProvider _requestCultureProvider;
    internal IHttpContextAccessor _httpContextAccessor;

    public LocalisationService(IHttpContextAccessor httpContextAccessor, IRequestCultureProvider requestCultureProvider)
    {
        _requestCultureProvider = requestCultureProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Load(string defaultLanguage, string rootLanguageDirectory, string pattern)
    {
        DefaultLanguage = defaultLanguage;
        _texts = new Dictionary<string, Dictionary<string, string>>();
        _files = new Dictionary<string, List<FileInfo>>();
        foreach (var file in Directory.GetFiles(rootLanguageDirectory, pattern, SearchOption.AllDirectories))
        {
            var fi = new FileInfo(file);
            var fileTab = fi.Name.Split('.');
            var language = fileTab[fileTab.Length - 2];
            LoadFile(language, fi);
        }
    }

    private void LoadFile(string language, FileInfo languageFile)
    {
        if(!_files.ContainsKey(language))
        {
            _files.Add(language, new List<FileInfo>());
        }
        _files[language].Add(languageFile);

        if (!_texts.ContainsKey(language))
        {
            _texts.Add(language, new Dictionary<string, string>());
        }

        var translations = File.ReadAllText(languageFile.FullName);
        LoadData(translations, language);
    }

    private void LoadData(string text, string language)
    {
        var regEx = new Regex("\\\"([\\w]+)\"[\\s]*[:][\\s]*\"(.*)\\\"");
        foreach (var match in regEx.Matches(text))
        {
            var key = ((Match)match).Groups[1].Value;
            var value = ((Match)match).Groups[2].Value;
            if (_texts[language].ContainsKey(key))
            {
                _texts[language][key] = value;
            }
            else
            {
                _texts[language].Add(key, value);
            }
        }
    }
    
    public async Task<IDictionary<string, string>> GetAllTexts()
    {
        var language = await DetermineLanguage();
        return _texts[language];
    }

    private async Task<string> DetermineLanguage()
    {
        var language = DefaultLanguage;
        if(_requestCultureProvider != null && _httpContextAccessor != null)
        {
            var result = (await _requestCultureProvider.DetermineProviderCultureResult(_httpContextAccessor.HttpContext));
            language =  result.Cultures.First().ToString();
        }
        if (string.IsNullOrEmpty(language) || !_texts.ContainsKey(language))
        {
            language = DefaultLanguage;
        }
        return language;
    }

    public async Task<string> GetText(string label)
    {
        var language = await DetermineLanguage();
        if (language == null || !_texts.ContainsKey(language))
        {
            language = DefaultLanguage;
        }

        if (_texts[language].ContainsKey(label))
        {
            return _texts[language][label];
        }

        if (_texts[DefaultLanguage].ContainsKey(label))
        {
            return _texts[DefaultLanguage][label];
        }

        return string.Empty;
    }
}