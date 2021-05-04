using Polyglot;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace SiraLocalizer
{
    internal class PolyglotUtil
    {
        public static async Task AddLocalizationFromResource(string resourceName)
        {
            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
            {
                string content = await reader.ReadToEndAsync();
                Localization.Instance.InputFiles.Add(new LocalizationAsset() { Format = GoogleDriveDownloadFormat.CSV, TextAsset = new TextAsset(content) });
            }
        }

        public static IEnumerable<string> GetKeysFromLocalizationAsset(LocalizationAsset localizationAsset)
        {
            return GetKeysFromLocalizationAsset(localizationAsset.TextAsset.text, localizationAsset.Format);
        }

        public static IEnumerable<string> GetKeysFromLocalizationAsset(string content, GoogleDriveDownloadFormat format)
        {
            List<List<string>> rows;
            
            if (format == GoogleDriveDownloadFormat.TSV)
            {
                rows = TsvReader.Parse(content);
            }
            else
            {
                rows = CsvReader.Parse(content);
            }

            return rows.SkipWhile(row => row[0] != "Polyglot").Skip(1).Select(row => row[0]);
        }
    }
}
