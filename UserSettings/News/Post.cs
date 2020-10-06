using Newtonsoft.Json;
using System;

namespace BetterLanis.UserSettings.News
{
    [Serializable]
    public class Post
    {
        [JsonProperty("viewed")]
        public bool Viewed { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("news")]
        public Article[] Articles { get; set; }
    }
}