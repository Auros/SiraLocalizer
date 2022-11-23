using Polyglot;
using UnityEngine;

namespace SiraLocalizer.Records
{
    public record LocalizationFile
    {
        internal LocalizationAsset localizationAsset { get; }

        internal int priority { get; }

        internal LocalizationFile(string content, int priority)
        {
            localizationAsset = new LocalizationAsset { TextAsset = new TextAsset(content), Format = GoogleDriveDownloadFormat.CSV };
            this.priority = priority;
        }

        internal LocalizationFile(LocalizationAsset localizationAsset, int priority)
        {
            this.localizationAsset = localizationAsset;
            this.priority = priority;
        }
    }
}
