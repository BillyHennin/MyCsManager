// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from MANAGER INC. team.
//  
// Copyrights (c) 2014 MANAGER INC. All rights reserved.

using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MANAGER.Classes
{
    public static class Transharp
    {
        public enum LangsEnum
        {
            Deutsch,
            English,
            French,
            Spanish
        }

        private const string LangsFolder = "Language"; // langs folder
        private const string LangFileExt = ".lang"; // File extension
        private const char Separator = '=';
        private const string Placeholder = "%x";
        private static LangsEnum _currentLanguage = LangsEnum.English; //Default

        public static void SetCurrentLanguage(LangsEnum lang)
        {
            _currentLanguage = lang;
        }

        public static string GetCurrentLanguage() 
            => _currentLanguage.ToString();

        public static string GetTranslation(string key) 
            => GetTranslation(key, _currentLanguage);

        public static string GetTranslation(string key, params object[] values) 
            => GetTranslation(key, _currentLanguage, values);

        private static string GetTranslation(string key, LangsEnum lang, params object[] values)
        {
            var strToFormat = GetTranslation(key, lang);
            if(strToFormat == null)
            {
                return $"#{key} not found"; // Translation not found for the given key and lang
            }
            var index = 0;
            //Replacing <Placeholder> by {0}, {1} etc
            strToFormat = Regex.Replace(strToFormat, @Placeholder, delegate { return "{" + index++ + "}"; });
            return string.Format(strToFormat, values); // Format and return the translation
        }

        private static string GetTranslation(string key, LangsEnum lang)
        {
            var filePath = GetLangFilePath(lang);
            const int bufferSize = 1024;
            using(var fileStream = File.OpenRead(filePath))
            {
                using(var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
                {
                    string line;
                    while((line = streamReader.ReadLine()) != null)
                    {
                        var parts = line.Split(new[] {Separator}, 2); // Split only on 1st occurence (we want 2 parts max)
                        var fileKey = parts[0]; // Left part of the separator
                        if(!key.Equals(fileKey))
                        {
                            continue; //Not the correct key, we go to next loop
                        }
                        return parts[1]; // Right part of the separator
                    }
                }
            }
            return null; // Translation not found for the given key and lang
        }

        private static string GetLangFilePath(LangsEnum lang)
        {
            var str = new StringBuilder();
            str.Append(LangsFolder).Append("/").Append(lang.ToString().ToLower()).Append(LangFileExt);
            return str.ToString();
        }
    }
}