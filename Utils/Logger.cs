using Microsoft.Extensions.Logging;
using AdvancedStatistics.Config;

namespace AdvancedStatistics.Utils
{
    public class Logger
    {
        public readonly PluginConfig _config;
        private ILogger? _logger;

        public Logger(PluginConfig config)
        {
            _config = config;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void LogInfo(string message)
        {
            if (_config.EnableLogging && _logger != null)
            {
                _logger.LogInformation($"[Advanced Statistics] {message}");
            }
        }

        public void LogWarning(string message)
        {
            if (_config.EnableLogging && _logger != null)
            {
                _logger.LogWarning($"[Advanced Statistics] {message}");
            }
        }

        public void LogError(string message)
        {
            if (_config.EnableLogging && _logger != null)
            {
                _logger.LogError($"[Advanced Statistics] {message}");
            }
        }

        public void LogPlayerKill(string attackerName, string victimName, string weapon, bool headshot)
        {
            if (_config.EnableLogging && _config.LogPlayerKills && _logger != null)
            {
                string headshotText = headshot ? " (HEADSHOT)" : "";
                _logger.LogInformation($"[Advanced Statistics] [KILL] {attackerName} killed {victimName} with {weapon}{headshotText}");
            }
        }

        public void LogPlayerDeath(string victimName, string attackerName, string weapon)
        {
            if (_config.EnableLogging && _config.LogPlayerDeaths && _logger != null)
            {
                _logger.LogInformation($"[Advanced Statistics] [DEATH] {victimName} was killed by {attackerName} with {weapon}");
            }
        }

        public void LogPlayerAssist(string assisterName, string victimName, string weapon)
        {
            if (_config.EnableLogging && _config.LogPlayerAssists && _logger != null)
            {
                _logger.LogInformation($"[Advanced Statistics] [ASSIST] {assisterName} assisted in killing {victimName} with {weapon}");
            }
        }

        public void LogPlayerConnect(string playerName, string steamId)
        {
            if (_config.EnableLogging && _config.LogPlayerConnect && _logger != null)
            {
                _logger.LogInformation($"[Advanced Statistics] [CONNECT] Player {playerName} ({steamId}) connected to the server");
            }
        }

        public void LogPlayerDisconnect(string playerName, string steamId)
        {
            if (_config.EnableLogging && _config.LogPlayerDisconnect && _logger != null)
            {
                _logger.LogInformation($"[Advanced Statistics] [DISCONNECT] Player {playerName} ({steamId}) disconnected from the server");
            }
        }

        public void LogRoundEnd(int winnerTeam)
        {
            if (_config.EnableLogging && _config.LogRoundEnd && _logger != null)
            {
                string teamName = winnerTeam switch
                {
                    2 => "Terrorists",
                    3 => "Counter-Terrorists",
                    _ => "Unknown"
                };
                _logger.LogInformation($"[Advanced Statistics] [ROUND_END] Round ended. Winner: {teamName} (Team {winnerTeam})");
            }
        }

        public void LogDatabaseOperation(string operation, string details = "")
        {
            if (_config.EnableLogging && _logger != null)
            {
                _logger.LogInformation($"[Advanced Statistics] [DATABASE] {operation}: {details}");
            }
        }

        public void LogStatsUpdate(string playerName, string statType, int value)
        {
            if (_config.EnableLogging && _logger != null)
            {
                _logger.LogInformation($"[Advanced Statistics] [STATS] {playerName}: {statType} updated to {value}");
            }
        }
    }
}