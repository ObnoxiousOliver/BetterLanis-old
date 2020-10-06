using Newtonsoft.Json;
using System;

namespace BetterLanis.Login.SchoolList
{
    [Serializable]
    class School
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Id")]
        public string Id { get; set; }
        [JsonProperty("Ort")]
        public string Local { get; set; }
    }
}