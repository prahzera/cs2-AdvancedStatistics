using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using AdvancedStatistics.Services;
using AdvancedStatistics.Utils;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API;

namespace AdvancedStatistics.Events
{
    public class WeaponTrackingEvents
    {
        private readonly StatsService _statsService;
        private readonly Logger _logger;
        private ILogger? _internalLogger;

        public bool IsWarmup { get; set; } = true;

        public WeaponTrackingEvents(StatsService statsService, Logger logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        public void SetLogger(ILogger logger)
        {
            _internalLogger = logger;
        }

        private bool ShouldTrackEvent(bool isBotInvolved = false)
        {
            // Si es calentamiento y no se deben rastrear eventos de calentamiento, no rastrear
            if (IsWarmup && !_logger._config.TrackWarmupEvents)
            {
                return false;
            }

            // Si involucra bots y no se deben rastrear eventos de bots, no rastrear
            if (isBotInvolved && !_logger._config.TrackBotEvents)
            {
                return false;
            }

            return true;
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

                // Verificar si debemos rastrear este evento
                if (!ShouldTrackEvent(attacker.IsBot || victim.IsBot))
                {
                    return HookResult.Continue;
                }

                // Rastrear la kill con hitgroup
                _statsService.TrackKillWithHitGroup(attacker, victim, weaponName, hitgroup);

                // Si hay un asistente, rastrear la asistencia
                if (@event.Assister != null && @event.Assister.IsValid && @event.Assister != attacker)
                {
                    _statsService.TrackAssist(@event.Assister, attacker, weaponName);
                }
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnPlayerDeath: {ex.Message}");
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

                // Verificar si debemos rastrear este evento
                if (!ShouldTrackEvent(attacker.IsBot || victim.IsBot))
                {
                    return HookResult.Continue;
                }

                // Actualizar estadísticas de daño
                UpdateDamageStats(attacker, victim, damage);

                _logger.LogInfo($"Player {attacker.PlayerName} hurt {victim.PlayerName} with {weaponName} for {damage} damage");
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnPlayerHurt: {ex.Message}");
            }

            return HookResult.Continue;
        }

        private void UpdateDamageStats(CCSPlayerController attacker, CCSPlayerController victim, int damage)
        {
            // Actualizar daño hecho por el atacante
            if (attacker != null && attacker.IsValid)
            {
                string attackerSteamId = attacker.SteamID.ToString();
                MainThreadDispatcher.Enqueue(() => {
                    try
                    {
                        if (_statsService._cachedStats.ContainsKey(attackerSteamId))
                        {
                            var attackerStats = _statsService._cachedStats[attackerSteamId];
                            attackerStats.TotalDamageDone += damage;
                            _logger.LogStatsUpdate(attacker.PlayerName, "Total Damage Done", attackerStats.TotalDamageDone);
                        }
                        else
                        {
                            // Si no existe en caché, crear nuevas estadísticas
                            var attackerStats = new AdvancedStatistics.Models.PlayerStats
                            {
                                SteamId = attackerSteamId,
                                PlayerName = attacker.PlayerName,
                                TotalDamageDone = damage,
                                LastUpdated = DateTime.UtcNow
                            };
                            _statsService._cachedStats[attackerSteamId] = attackerStats;
                            _logger.LogStatsUpdate(attacker.PlayerName, "Total Damage Done", attackerStats.TotalDamageDone);
                        }
                    }
                    catch (Exception ex)
                    {
                        _internalLogger?.LogError($"Error updating attacker damage stats: {ex.Message}");
                    }
                });
            }

            // Actualizar daño recibido por la víctima
            if (victim != null && victim.IsValid)
            {
                string victimSteamId = victim.SteamID.ToString();
                MainThreadDispatcher.Enqueue(() => {
                    try
                    {
                        if (_statsService._cachedStats.ContainsKey(victimSteamId))
                        {
                            var victimStats = _statsService._cachedStats[victimSteamId];
                            victimStats.TotalDamageTaken += damage;
                            _logger.LogStatsUpdate(victim.PlayerName, "Total Damage Taken", victimStats.TotalDamageTaken);
                        }
                        else
                        {
                            // Si no existe en caché, crear nuevas estadísticas
                            var victimStats = new AdvancedStatistics.Models.PlayerStats
                            {
                                SteamId = victimSteamId,
                                PlayerName = victim.PlayerName,
                                TotalDamageTaken = damage,
                                LastUpdated = DateTime.UtcNow
                            };
                            _statsService._cachedStats[victimSteamId] = victimStats;
                            _logger.LogStatsUpdate(victim.PlayerName, "Total Damage Taken", victimStats.TotalDamageTaken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _internalLogger?.LogError($"Error updating victim damage stats: {ex.Message}");
                    }
                });
            }
        }

        public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            try
            {
                // Verificar si debemos rastrear este evento
                if (IsWarmup && !_logger._config.TrackWarmupEvents)
                {
                    return HookResult.Continue;
                }

                // Incrementar el contador de rondas jugadas para todos los jugadores conectados
                UpdateRoundsPlayed();

                // Guardar todas las estadísticas al final de la ronda
                // Esto asegura que los datos se guarden periódicamente
                _statsService.SaveAllStats();
                
                // Loggear el evento
                _logger.LogRoundEnd(@event.Winner);
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnRoundEnd: {ex.Message}");
            }

            return HookResult.Continue;
        }

        private void UpdateRoundsPlayed()
        {
            MainThreadDispatcher.Enqueue(() => {
                try
                {
                    // Obtener todos los jugadores conectados
                    var players = Utilities.GetPlayers();
                    
                    foreach (var player in players)
                    {
                        if (player != null && player.IsValid && !player.IsBot)
                        {
                            string steamId = player.SteamID.ToString();
                            
                            // Verificar si el jugador está en caché, si no, crear nuevas estadísticas
                            if (!_statsService._cachedStats.ContainsKey(steamId))
                            {
                                var newStats = new AdvancedStatistics.Models.PlayerStats
                                {
                                    SteamId = steamId,
                                    PlayerName = player.PlayerName,
                                    LastUpdated = DateTime.UtcNow
                                };
                                _statsService._cachedStats[steamId] = newStats;
                            }
                            
                            // Incrementar rondas jugadas
                            _statsService._cachedStats[steamId].TotalRoundsPlayed++;
                            _logger.LogStatsUpdate(player.PlayerName, "Total Rounds Played", _statsService._cachedStats[steamId].TotalRoundsPlayed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _internalLogger?.LogError($"Error updating rounds played: {ex.Message}");
                }
            });
        }

        public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            try
            {
                var player = @event.Userid;
                
                if (player != null && player.IsValid)
                {
                    // Cargar estadísticas del jugador al conectarse
                    // Esta operación puede permanecer asíncrona ya que no modifica datos críticos
                    _ = _statsService.GetOrCreatePlayerStatsAsync(player);
                    
                    // Loggear el evento
                    _logger.LogPlayerConnect(player.PlayerName, player.SteamID.ToString());
                }
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnPlayerConnectFull: {ex.Message}");
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
                    _statsService.SavePlayerStats(player);
                    
                    // Loggear el evento
                    _logger.LogPlayerDisconnect(player.PlayerName, player.SteamID.ToString());
                }
            }
            catch (Exception ex)
            {
                _internalLogger?.LogError($"Error in OnPlayerDisconnect: {ex.Message}");
            }

            return HookResult.Continue;
        }
    }
}