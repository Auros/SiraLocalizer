namespace SiraLocalizer.Records
{
    internal record TranslationStatus
    {
        public string name { get; }

        public int totalStrings { get; }

        public int translatedStrings { get; }

        public float percentTranslated { get; }

        public TranslationStatus(string name, int totalStrings, int translatedStrings)
        {
            this.name = name;
            this.totalStrings = totalStrings;
            this.translatedStrings = translatedStrings;
            percentTranslated = totalStrings > 0 ? 100f * translatedStrings / totalStrings : 0;
        }
    }
}
