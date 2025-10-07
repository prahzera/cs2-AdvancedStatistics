using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using AdvancedStatistics.Config;
using AdvancedStatistics.Database;
using AdvancedStatistics.Services;

namespace AdvancedStatistics
{
    public class AdvancedStatisticsPlugin : BasePlugin
    {
        public override string ModuleName => "Advanced Statistics";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "AdvancedStats Team";

        private PluginConfig _config = new PluginConfig();
        private DatabaseManager _databaseManager = null!;
        private StatsService _statsService = null!;

        public override void Load(bool hotReload)
        {
            // Cargar configuración
            _config = ConfigManager.Load<PluginConfig>(ModuleName);
            
            // Inicializar componentes
            _databaseManager = new DatabaseManager(_config);
            _statsService = new StatsService(_databaseManager);
            
            // Registrar eventos
            RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            
            Logger.LogInformation($"Advanced Statistics plugin loaded. Tracking bot events: {_config.TrackBotEvents}");
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            // Implementar lógica de seguimiento de muertes
            return HookResult.Continue;
        }

        private HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            // Implementar lógica de seguimiento de daño
            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            // Implementar lógica al finalizar la ronda
            return HookResult.Continue;
        }

        public override void Unload(bool hotReload)
        {
            Logger.LogInformation("Advanced Statistics plugin unloaded.");
        }
    }
}