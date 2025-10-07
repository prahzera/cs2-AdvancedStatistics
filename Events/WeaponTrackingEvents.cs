using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using AdvancedStatistics.Services;
using Microsoft.Extensions.Logging;

namespace AdvancedStatistics.Events
{
    public class WeaponTrackingEvents
    {
        private readonly StatsService _statsService;
        private ILogger? _logger;

        public WeaponTrackingEvents(StatsService statsService)
        {
            _statsService = statsService;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            try
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;
                var weaponName = @event.Weapon;
                var headshot = @event.Headshot;
                var hitgroup = @event.Hitgroup; // Nuevo campo para el hitgroup

                // Validar que ambos jugadores sean válidos
                if (attacker == null || victim == null || !attacker.IsValid || !victim.IsValid)
                {
                    return HookResult.Continue;
                }

                // No contar muertes por el mismo jugador (suicidios)
                if (attacker == victim)
                {
                    return HookResult.Continue;
                }

                // Verificar que ambos sean jugadores humanos o que esté permitido rastrear bots
                // Esta verificación se hará dentro del servicio de estadísticas

                // Rastrear la kill con hitgroup
                _statsService.TrackKillWithHitGroup(attacker, victim, weaponName, hitgroup);

                // Si hay un asistente, rastrear la asistencia
                if (@event.Assister != null && @event.Assister.IsValid && @event.Assister != attacker)
                {
                    _statsService.TrackAssist(@event.Assister, weaponName);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnPlayerDeath: {ex.Message}");
            }

            return HookResult.Continue;
        }

        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            try
            {
                var attacker = @event.Attacker;
                var victim = @event.Userid;
                var weaponName = @event.Weapon;
                var damage = @event.DmgHealth;

                // Validar que ambos jugadores sean válidos
                if (attacker == null || victim == null || !attacker.IsValid || !victim.IsValid)
                {
                    return HookResult.Continue;
                }

                // No contar daño autoinfligido
                if (attacker == victim)
                {
                    return HookResult.Continue;
                }

                // Actualizar estadísticas de daño
                // Esta funcionalidad se puede implementar en el servicio de estadísticas
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnPlayerHurt: {ex.Message}");
            }

            return HookResult.Continue;
        }

        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            try
            {
                // Guardar todas las estadísticas al final de la ronda
                // Esto asegura que los datos se guarden periódicamente
                var saveTask = _statsService.SaveAllStatsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnRoundEnd: {ex.Message}");
            }

            return HookResult.Continue;
        }

        public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            try
            {
                var player = @event.Userid;
                
                if (player != null && player.IsValid)
                {
                    // Cargar estadísticas del jugador al conectarse
                    var loadTask = _statsService.GetOrCreatePlayerStatsAsync(player);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnPlayerConnectFull: {ex.Message}");
            }

            return HookResult.Continue;
        }

        public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            try
            {
                var player = @event.Userid;
                
                if (player != null && player.IsValid)
                {
                    // Guardar estadísticas del jugador al desconectarse
                    var saveTask = _statsService.SavePlayerStatsAsync(player);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error in OnPlayerDisconnect: {ex.Message}");
            }

            return HookResult.Continue;
        }
    }
}