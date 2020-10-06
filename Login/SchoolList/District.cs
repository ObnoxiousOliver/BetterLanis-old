using Newtonsoft.Json;
using System;

namespace BetterLanis.Login.SchoolList
{
    [Serializable]
    class District
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("Schulen")]
        public School[] Schools { get; set; }
    }
}