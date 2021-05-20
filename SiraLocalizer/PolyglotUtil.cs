using Polyglot;
using System.Collections.Generic;
using System.Linq;

namespace SiraLocalizer
{
    internal class PolyglotUtil
    {
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
