using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using AdvancedStatistics.Services;
using AdvancedStatistics.Utils;
using Microsoft.Extensions.Logging;

namespace AdvancedStatistics.Commands
{
    public class StatsCommands
    {
        private readonly StatsService _statsService;
        private readonly Logger _logger;
        private ILogger? _internalLogger;

        public StatsCommands(StatsService statsService, Logger logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        public void SetLogger(ILogger logger)
        {
            _internalLogger = logger;
        }

        [CommandHelper(minArgs: 0, usage: "[<steamid>|<name>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnStatsCommand(CCSPlayerController? player, CommandInfo command)
        {
            try
            {
                // Implementar comando para mostrar estadísticas
                command.ReplyToCommand("Estadísticas del jugador: [Funcionalidad pendiente de implementar]");
                _logger.LogInfo($"Player {player?.PlayerName ?? "Console"} requested stats");
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnStatsCommand: {ex.Message}");
                command.ReplyToCommand("Error al obtener estadísticas");
            }
        }

        [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnTopStatsCommand(CCSPlayerController? player, CommandInfo command)
        {
            try
            {
                // Implementar comando para mostrar top estadísticas
                command.ReplyToCommand("Top estadísticas: [Funcionalidad pendiente de implementar]");
                _logger.LogInfo($"Player {player?.PlayerName ?? "Console"} requested top stats");
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnTopStatsCommand: {ex.Message}");
                command.ReplyToCommand("Error al obtener top estadísticas");
            }
        }

        [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.SERVER_ONLY)]
        public void OnResetStatsCommand(CCSPlayerController? player, CommandInfo command)
        {
            try
            {
                // Implementar comando para resetear estadísticas
                command.ReplyToCommand("Estadísticas reseteadas: [Funcionalidad pendiente de implementar]");
                _logger.LogInfo($"Player {player?.PlayerName ?? "Console"} requested stats reset");
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnResetStatsCommand: {ex.Message}");
                command.ReplyToCommand("Error al resetear estadísticas");
            }
        }
    }
}