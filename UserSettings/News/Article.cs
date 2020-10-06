using Newtonsoft.Json;
using System;

namespace BetterLanis.UserSettings.News
{
    [Serializable]
    public class Article
    {
        [JsonProperty("local")]
        public string Local { get; set; }
        [JsonProperty("localName")]
        public string LocalName { get; set; }
        [JsonProperty("header")]
        public string Header { get; set; }
        [JsonProperty("text")]
        public string[] Text { get; set; }
    }
}