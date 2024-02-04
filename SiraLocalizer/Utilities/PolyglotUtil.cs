using System.Collections.Generic;
using System.Linq;
using BGLib.Polyglot;
using UnityEngine;

namespace SiraLocalizer.Utilities
{
    internal class PolyglotUtil
    {
        public static IEnumerable<string> GetKeysFromLocalizationAsset(IEnumerable<TextAsset> textAssets)
        {
            return textAssets.SelectMany(asset => GetKeysFromLocalizationAsset(asset.text, GoogleDriveDownloadFormat.CSV));
        }

        public static IEnumerable<string> GetKeysFromLocalizationAsset(string content, GoogleDriveDownloadFormat format)
        {
            List<List<string>> rows = format == GoogleDriveDownloadFormat.TSV ? TsvReader.Parse(content) : CsvReader.Parse(content);
            return rows.SkipWhile(row => row[0] != "Polyglot").Skip(1).Select(row => row[0]);
        }
    }
}
