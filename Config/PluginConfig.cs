using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace AdvancedStatistics.Config
{
    public class PluginConfig : BasePluginConfig
    {
        [JsonPropertyName("DatabaseHost")]
        public string DatabaseHost { get; set; } = "localhost";

        [JsonPropertyName("DatabasePort")]
        public int DatabasePort { get; set; } = 3306;

        [JsonPropertyName("DatabaseName")]
        public string DatabaseName { get; set; } = "cs2_stats";

        [JsonPropertyName("DatabaseUser")]
        public string DatabaseUser { get; set; } = "root";

        [JsonPropertyName("DatabasePassword")]
        public string DatabasePassword { get; set; } = "";

        [JsonPropertyName("TrackBotEvents")]
        public bool TrackBotEvents { get; set; } = false;

        [JsonPropertyName("UpdateInterval")]
        public int UpdateInterval { get; set; } = 30;

        [JsonPropertyName("EnableLogging")]
        public bool EnableLogging { get; set; } = true;

        [JsonPropertyName("LogPlayerKills")]
        public bool LogPlayerKills { get; set; } = true;

        [JsonPropertyName("LogPlayerDeaths")]
        public bool LogPlayerDeaths { get; set; } = true;

        [JsonPropertyName("LogPlayerAssists")]
        public bool LogPlayerAssists { get; set; } = true;

        [JsonPropertyName("LogPlayerConnect")]
        public bool LogPlayerConnect { get; set; } = true;

        [JsonPropertyName("LogPlayerDisconnect")]
        public bool LogPlayerDisconnect { get; set; } = true;

        [JsonPropertyName("LogRoundEnd")]
        public bool LogRoundEnd { get; set; } = true;
    }
}