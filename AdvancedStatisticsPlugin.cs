using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using AdvancedStatistics.Config;
using AdvancedStatistics.Database;
using AdvancedStatistics.Services;
using AdvancedStatistics.Events;
using AdvancedStatistics.Commands;
using CounterStrikeSharp.API.Modules.Config;

namespace AdvancedStatistics
{
    public class AdvancedStatisticsPlugin : BasePlugin
    {
        public override string ModuleName => "AdvancedStatistics";
        public override string ModuleVersion => "1.2.1";
        public override string ModuleAuthor => "Prahzera";

        private PluginConfig _config = new PluginConfig();
        private DatabaseManager _databaseManager = null!;
        private StatsService _statsService = null!;
        private WeaponTrackingEvents _weaponTrackingEvents = null!;
        private StatsCommands _statsCommands = null!;

        public override void Load(bool hotReload)
        {
            // Cargar configuración usando ConfigManager
            // El framework de Counter-Strike Sharp creará automáticamente
            // el archivo de configuración en la primera ejecución si no existe
            _config = ConfigManager.Load<PluginConfig>(ModuleName);
            
            // Inicializar componentes
            _databaseManager = new DatabaseManager(_config);
            _statsService = new StatsService(_databaseManager);
            _statsService.SetLogger(Logger);
            
            // Inicializar eventos
            _weaponTrackingEvents = new WeaponTrackingEvents(_statsService);
            _weaponTrackingEvents.SetLogger(Logger);
            
            // Inicializar comandos
            _statsCommands = new StatsCommands(_statsService);
            _statsCommands.SetLogger(Logger);
            
            // Registrar eventos
            RegisterEventHandler<EventPlayerDeath>(_weaponTrackingEvents.OnPlayerDeath);
            RegisterEventHandler<EventPlayerHurt>(_weaponTrackingEvents.OnPlayerHurt);
            RegisterEventHandler<EventRoundEnd>(_weaponTrackingEvents.OnRoundEnd);
            RegisterEventHandler<EventPlayerConnectFull>(_weaponTrackingEvents.OnPlayerConnectFull);
            RegisterEventHandler<EventPlayerDisconnect>(_weaponTrackingEvents.OnPlayerDisconnect);
            
            // Registrar comandos
            AddCommand("css_stats", "Muestra las estadísticas del jugador", _statsCommands.OnStatsCommand);
            AddCommand("css_topstats", "Muestra las mejores estadísticas", _statsCommands.OnTopStatsCommand);
            AddCommand("css_resetstats", "Reinicia las estadísticas (solo admin)", _statsCommands.OnResetStatsCommand);
            
            Logger.LogInformation("Advanced Statistics plugin loaded successfully.");
        }

        public override void Unload(bool hotReload)
        {
            Logger.LogInformation("Advanced Statistics plugin unloaded.");
        }
    }
}