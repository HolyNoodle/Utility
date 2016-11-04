using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HolyNoodle.Utility
{
    public static class LocalisationHelper
    {
        private static Dictionary<string, Dictionary<string, string>> _texts;
        private static Dictionary<string, FileInfo> _files;

        public static string DefaultLanguage { get; private set; }

        public static ILocalisationProvider Provider { get; set; }

        public static string LanguageFilePath { get; set; }

        private static void LoadFile(string language, FileInfo languageFile)
        {
            _files.Add(language, languageFile);

            if (!_texts.ContainsKey(language))
            {
                _texts.Add(language, new Dictionary<string, string>());
            }
            else
            {
                _texts[language].Clear();
            }

            var translations = File.ReadAllText(languageFile.FullName);
            LoadData(translations, language);
        }

        private static void LoadData(string text, string language)
        {
            _texts[language].Clear();
            var regEx = new Regex("\\\"([\\w]+)\"[\\s]*[:][\\s]*\"(.*)\\\"");
            foreach (var match in regEx.Matches(text))
            {
                var key = ((Match)match).Groups[1].Value;
                var value = ((Match)match).Groups[2].Value;
                _texts[language].Add(key, value);
            }
        }

        public static string GetText(string label)
        {
            var language = Provider.GetLanguage().Name.ToLower().Split('-')[0];
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

        public static void Init(string defaultLanguage, ApplicationType applicationType = ApplicationType.StandAlone, string languageFileDirectory = "")
        {
            _texts = new Dictionary<string, Dictionary<string, string>>();
            _files = new Dictionary<string, FileInfo>();
            DefaultLanguage = defaultLanguage;
            switch (applicationType)
            {
                case ApplicationType.Web:
                    Provider = new WebLocalisationProvider();
                    break;
                default:
                    Provider = new LocalisationProvider();
                    break;
            }

            var filesPath = Utility.AssemblyDirectory;
            if (languageFileDirectory != string.Empty)
                filesPath = languageFileDirectory;

            foreach (var file in Directory.GetFiles(filesPath, "language.*.json", SearchOption.AllDirectories))
            {
                var fi = new FileInfo(file);
                var fileTab = fi.Name.Split('.');
                var language = fileTab[fileTab.Length - 2];
                LoadFile(language, fi);
            }
        }

        public static void AddTranslation(string filePath)
        {
            var fi = new FileInfo(filePath);
            var fileTab = fi.Name.Split('.');
            var language = fileTab[fileTab.Length - 2];

            if (!_texts.ContainsKey(language))
            {
                _texts.Add(language, new Dictionary<string, string>());
            }

            var translations = File.ReadAllText(fi.FullName);
            LoadData(translations, language);
        }

        public static IEnumerable<CultureInfo> GetLanguages()
        {
            foreach (var lang in _texts.Keys)
            {
                yield return new CultureInfo(lang);
            }
        }

        public static void ChangeLanguage(CultureInfo culture)
        {
            Provider.SetLanguage(culture);
        }
    }
}
