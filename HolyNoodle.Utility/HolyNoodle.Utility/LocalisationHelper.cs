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
        private static Dictionary<FileInfo, DateTime> _files;

        public static string DefaultLanguage { get; private set; }

        public static ILocalisationProvider Provider { get; set; }

        public static string LanguageFilePath { get; set; }

        private static void LoadFile(string language, FileInfo languageFile = null)
        {
            //If language file is not given, we already have the data
            //Then get the language file from the _files dictionary
            //else initialise the language file
            if(languageFile == null)
            {
                languageFile = _files.FirstOrDefault(kv => kv.Key.Name == "language." + language + ".json").Key;
            }
            else
            {
                _files.Add(languageFile, DateTime.MinValue);
            }
            var lastWriteDate = new FileInfo(languageFile.FullName).LastWriteTimeUtc;

            var lastRefreshed = _files[languageFile];
            if (lastWriteDate.Millisecond != lastRefreshed.Millisecond)
            {
                if (!_texts.ContainsKey(language))
                {
                    _texts.Add(language, new Dictionary<string, string>());
                }
                var translations = File.ReadAllText(languageFile.FullName);
                _files[languageFile] = lastWriteDate;
                LoadData(translations, language);
            }
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
            if(language == null || !_texts.ContainsKey(language))
            {
                language = DefaultLanguage;
            }
            LoadFile(language);
            
            if(_texts[language].ContainsKey(label))
            {
                return _texts[language][label];
            }

            if(_texts[DefaultLanguage].ContainsKey(label))
            {
                return _texts[DefaultLanguage][label];
            }

            return string.Empty;
        }

        public static void Init(string defaultLanguage, ApplicationType applicationType = ApplicationType.StandAlone, string languageFilePath = "")
        {
            _texts = new Dictionary<string, Dictionary<string, string>>();
            _files = new Dictionary<FileInfo, DateTime>();
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
            if (languageFilePath != string.Empty)
                filesPath = languageFilePath;

            foreach (var file in Directory.GetFiles(filesPath, "language.*.json", SearchOption.AllDirectories))
            {                
                var fileTab = file.Split('.');
                var language = fileTab[fileTab.Length - 2];
                LoadFile(language, new FileInfo(file));
            }
        }

        public static IEnumerable<CultureInfo> GetLanguages()
        {
            foreach(var lang in _texts.Keys)
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
