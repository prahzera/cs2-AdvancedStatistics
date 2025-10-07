using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using AdvancedStatistics.Services;
using Microsoft.Extensions.Logging;

namespace AdvancedStatistics.Commands
{
    public class StatsCommands
    {
        private readonly StatsService _statsService;
        private ILogger? _logger;

        public StatsCommands(StatsService statsService)
        {
            _statsService = statsService;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        [CommandHelper(minArgs: 0, usage: "[<steamid>|<name>]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void OnStatsCommand(CCSPlayerController? player, CommandInfo command)
        {
            try
            {
                // Implementar comando para mostrar estadísticas
                command.ReplyToCommand("Estadísticas del jugador: [Funcionalidad pendiente de implementar]");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnStatsCommand: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnTopStatsCommand: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnResetStatsCommand: {ex.Message}");
                command.ReplyToCommand("Error al resetear estadísticas");
            }
        }
    }
}