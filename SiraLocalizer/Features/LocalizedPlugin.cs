using Newtonsoft.Json;

namespace SiraLocalizer.Features
{
    internal class LocalizedPlugin
    {
        [JsonProperty(Required = Required.Always)] public string id;
    }
}
