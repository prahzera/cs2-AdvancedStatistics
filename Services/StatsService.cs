using AdvancedStatistics.Database;
using AdvancedStatistics.Models;
using AdvancedStatistics.Utils;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using Microsoft.Extensions.Logging;

namespace AdvancedStatistics.Services
{
    public class StatsService
    {
        private readonly DatabaseManager _databaseManager;
        private readonly Logger _logger;
        private ILogger? _internalLogger;
        public Dictionary<string, PlayerStats> _cachedStats = new();

        public StatsService(DatabaseManager databaseManager, Logger logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        public void SetLogger(ILogger logger)
        {
            _internalLogger = logger;
            _databaseManager.SetLogger(logger);
        }

        public async Task InitializeAsync()
        {
            await _databaseManager.InitializeDatabaseAsync();
        }

        public async Task<PlayerStats> GetOrCreatePlayerStatsAsync(CCSPlayerController player)
        {
            var steamId = player.SteamID.ToString();
            
            // Verificar si ya tenemos en caché
            if (_cachedStats.ContainsKey(steamId))
            {
                return _cachedStats[steamId];
            }
            
            // Intentar obtener de la base de datos
            var stats = await _databaseManager.GetPlayerStatsAsync(steamId);
            
            if (stats == null)
            {
                // Crear nuevas estadísticas
                stats = new PlayerStats
                {
                    SteamId = steamId,
                    PlayerName = player.PlayerName,
                    LastUpdated = DateTime.UtcNow
                };
                
                _logger.LogInfo($"Created new stats for player {player.PlayerName} ({steamId})");
            }
            else
            {
                _logger.LogInfo($"Loaded existing stats for player {player.PlayerName} ({steamId})");
            }
            
            _cachedStats[steamId] = stats;
            return stats;
        }

        public void SavePlayerStats(CCSPlayerController player)
        {
            // Verificar que el jugador sea válido antes de encolar la operación
            if (player == null || !player.IsValid)
            {
                return;
            }
            
            // Obtener los datos necesarios mientras el jugador aún es válido
            string playerName = player.PlayerName;
            string steamId = player.SteamID.ToString();
            
            // Enqueue the operation to run on the main thread
            MainThreadDispatcher.Enqueue(() => {
                try
                {
                    if (_cachedStats.ContainsKey(steamId))
                    {
                        var stats = _cachedStats[steamId];
                        stats.PlayerName = playerName;
                        stats.LastUpdated = DateTime.UtcNow;
                        
                        // Actualizar estadísticas en la base de datos
                        _databaseManager.UpdatePlayerStatsAsync(stats).Wait();
                        _logger.LogInfo($"Saved stats for player {playerName} ({steamId})");
                    }
                }
                catch (Exception ex)
                {
                    _internalLogger?.LogError($"Error saving player stats: {ex.Message}");
                }
            });
        }

        public void TrackKill(CCSPlayerController attacker, CCSPlayerController victim, string weaponName, bool headshot)
        {
            // Verificar si debemos rastrear eventos de bots
            if ((attacker.IsBot || victim.IsBot) && !_logger._config.TrackBotEvents)
            {
                return;
            }
            
            // Actualizar estadísticas del atacante
            UpdateAttackerStats(attacker, weaponName, headshot);
            
            // Actualizar estadísticas de la víctima
            UpdateVictimStats(victim, weaponName, attacker.PlayerName);
            
            // Loggear el evento
            _logger.LogPlayerKill(attacker.PlayerName, victim.PlayerName, weaponName, headshot);
        }

        public void TrackKillWithHitGroup(CCSPlayerController attacker, CCSPlayerController victim, string weaponName, int hitGroup)
        {
            // Verificar si debemos rastrear eventos de bots
            if ((attacker.IsBot || victim.IsBot) && !_logger._config.TrackBotEvents)
            {
                return;
            }
            
            // Actualizar estadísticas del atacante con hitgroup
            UpdateAttackerStatsWithHitGroup(attacker, weaponName, hitGroup);
            
            // Actualizar estadísticas de la víctima
            UpdateVictimStats(victim, weaponName, attacker.PlayerName);
            
            // Loggear el evento
            string hitGroupText = hitGroup switch
            {
                1 => "Head",
                2 => "Chest",
                3 => "Stomach",
                4 => "Left Arm",
                5 => "Right Arm",
                6 => "Left Leg",
                7 => "Right Leg",
                _ => "Unknown"
            };
            
            _logger.LogPlayerKill(attacker.PlayerName, victim.PlayerName, $"{weaponName} ({hitGroupText})", hitGroup == 1);
        }

        public void TrackAssist(CCSPlayerController assister, CCSPlayerController attacker, string weaponName)
        {
            if (assister.IsBot && !_logger._config.TrackBotEvents)
            {
                return;
            }
            
            UpdateAssistStats(assister, weaponName, attacker.PlayerName);
            
            // Loggear el evento
            _logger.LogPlayerAssist(assister.PlayerName, attacker.PlayerName, weaponName);
        }

        private void UpdateAttackerStats(CCSPlayerController attacker, string weaponName, bool headshot)
        {
            // Verificar que el jugador sea válido antes de encolar la operación
            if (attacker == null || !attacker.IsValid)
            {
                return;
            }
            
            // Obtener los datos necesarios mientras el jugador aún es válido
            string attackerName = attacker.PlayerName;
            string steamId = attacker.SteamID.ToString();
            
            // Enqueue the operation to run on the main thread
            MainThreadDispatcher.Enqueue(() => {
                try
                {
                    // Verificar si ya tenemos en caché
                    PlayerStats? stats = null;
                    if (_cachedStats.ContainsKey(steamId))
                    {
                        stats = _cachedStats[steamId];
                    }
                    
                    // Si no tenemos stats en caché, crear nuevas estadísticas
                    if (stats == null)
                    {
                        stats = new PlayerStats
                        {
                            SteamId = steamId,
                            PlayerName = attackerName,
                            LastUpdated = DateTime.UtcNow
                        };
                        _cachedStats[steamId] = stats;
                    }
                    
                    stats.TotalKills++;
                    _logger.LogStatsUpdate(attackerName, "Total Kills", stats.TotalKills);
                    
                    if (headshot)
                    {
                        stats.TotalHeadshots++;
                        _logger.LogStatsUpdate(attackerName, "Total Headshots", stats.TotalHeadshots);
                    }
                    
                    // Incrementar kills específicos por arma
                    switch (weaponName.ToLower())
                    {
                        case "ak47":
                            stats.AK47Kills++;
                            _logger.LogStatsUpdate(attackerName, "AK47 Kills", stats.AK47Kills);
                            if (headshot) stats.AK47Headshots++;
                            break;
                        case "m4a4":
                            stats.M4A4Kills++;
                            _logger.LogStatsUpdate(attackerName, "M4A4 Kills", stats.M4A4Kills);
                            if (headshot) stats.M4A4Headshots++;
                            break;
                        case "m4a1_silencer":
                        case "m4a1_silencer_off":
                            stats.M4A1SKills++;
                            _logger.LogStatsUpdate(attackerName, "M4A1S Kills", stats.M4A1SKills);
                            if (headshot) stats.M4A1SHeadshots++;
                            break;
                        case "aug":
                            stats.AUGKills++;
                            _logger.LogStatsUpdate(attackerName, "AUG Kills", stats.AUGKills);
                            if (headshot) stats.AUGHeadshots++;
                            break;
                        case "sg556":
                            stats.SG553Kills++;
                            _logger.LogStatsUpdate(attackerName, "SG553 Kills", stats.SG553Kills);
                            if (headshot) stats.SG553Headshots++;
                            break;
                        case "galilar":
                            stats.GalilARKills++;
                            _logger.LogStatsUpdate(attackerName, "GalilAR Kills", stats.GalilARKills);
                            if (headshot) stats.GalilARHeadshots++;
                            break;
                        case "famas":
                            stats.FAMASKills++;
                            _logger.LogStatsUpdate(attackerName, "FAMAS Kills", stats.FAMASKills);
                            if (headshot) stats.FAMASHeadshots++;
                            break;
                        case "awp":
                            stats.AWPKills++;
                            _logger.LogStatsUpdate(attackerName, "AWP Kills", stats.AWPKills);
                            if (headshot) stats.AWPHeadshots++;
                            break;
                        case "ssg08":
                            stats.SSG08Kills++;
                            _logger.LogStatsUpdate(attackerName, "SSG08 Kills", stats.SSG08Kills);
                            if (headshot) stats.SSG08Headshots++;
                            break;
                        case "scar20":
                            stats.Scar20Kills++;
                            _logger.LogStatsUpdate(attackerName, "Scar20 Kills", stats.Scar20Kills);
                            if (headshot) stats.Scar20Headshots++;
                            break;
                        case "g3sg1":
                            stats.G3SG1Kills++;
                            _logger.LogStatsUpdate(attackerName, "G3SG1 Kills", stats.G3SG1Kills);
                            if (headshot) stats.G3SG1Headshots++;
                            break;
                        case "deagle":
                            stats.DeagleKills++;
                            _logger.LogStatsUpdate(attackerName, "Deagle Kills", stats.DeagleKills);
                            if (headshot) stats.DeagleHeadshots++;
                            break;
                        case "glock":
                            stats.GlockKills++;
                            _logger.LogStatsUpdate(attackerName, "Glock Kills", stats.GlockKills);
                            if (headshot) stats.GlockHeadshots++;
                            break;
                        case "usp_silencer":
                        case "usp_silencer_off":
                            stats.USPSKills++;
                            _logger.LogStatsUpdate(attackerName, "USP-S Kills", stats.USPSKills);
                            if (headshot) stats.USPSHeadshots++;
                            break;
                        case "p250":
                            stats.P250Kills++;
                            _logger.LogStatsUpdate(attackerName, "P250 Kills", stats.P250Kills);
                            if (headshot) stats.P250Headshots++;
                            break;
                        case "hkp2000":
                            stats.P2000Kills++;
                            _logger.LogStatsUpdate(attackerName, "P2000 Kills", stats.P2000Kills);
                            if (headshot) stats.P2000Headshots++;
                            break;
                        case "fiveseven":
                            stats.FiveSevenKills++;
                            _logger.LogStatsUpdate(attackerName, "FiveSeven Kills", stats.FiveSevenKills);
                            if (headshot) stats.FiveSevenHeadshots++;
                            break;
                        case "tec9":
                            stats.Tec9Kills++;
                            _logger.LogStatsUpdate(attackerName, "Tec9 Kills", stats.Tec9Kills);
                            if (headshot) stats.Tec9Headshots++;
                            break;
                        case "cz75a":
                            stats.CZ75Kills++;
                            _logger.LogStatsUpdate(attackerName, "CZ75 Kills", stats.CZ75Kills);
                            if (headshot) stats.CZ75Headshots++;
                            break;
                        case "elite":
                            stats.DualiesKills++;
                            _logger.LogStatsUpdate(attackerName, "Dualies Kills", stats.DualiesKills);
                            if (headshot) stats.DualiesHeadshots++;
                            break;
                        case "revolver":
                            stats.RevolverKills++;
                            _logger.LogStatsUpdate(attackerName, "Revolver Kills", stats.RevolverKills);
                            if (headshot) stats.RevolverHeadshots++;
                            break;
                        case "nova":
                            stats.NovaKills++;
                            _logger.LogStatsUpdate(attackerName, "Nova Kills", stats.NovaKills);
                            break;
                        case "xm1014":
                            stats.XM1014Kills++;
                            _logger.LogStatsUpdate(attackerName, "XM1014 Kills", stats.XM1014Kills);
                            break;
                        case "sawedoff":
                            stats.SawedOffKills++;
                            _logger.LogStatsUpdate(attackerName, "SawedOff Kills", stats.SawedOffKills);
                            break;
                        case "mag7":
                            stats.MAG7Kills++;
                            _logger.LogStatsUpdate(attackerName, "MAG7 Kills", stats.MAG7Kills);
                            break;
                        case "mac10":
                            stats.Mac10Kills++;
                            _logger.LogStatsUpdate(attackerName, "Mac10 Kills", stats.Mac10Kills);
                            break;
                        case "mp9":
                            stats.MP9Kills++;
                            _logger.LogStatsUpdate(attackerName, "MP9 Kills", stats.MP9Kills);
                            break;
                        case "mp7":
                            stats.MP7Kills++;
                            _logger.LogStatsUpdate(attackerName, "MP7 Kills", stats.MP7Kills);
                            break;
                        case "ump45":
                            stats.UMP45Kills++;
                            _logger.LogStatsUpdate(attackerName, "UMP45 Kills", stats.UMP45Kills);
                            break;
                        case "p90":
                            stats.P90Kills++;
                            _logger.LogStatsUpdate(attackerName, "P90 Kills", stats.P90Kills);
                            break;
                        case "bizon":
                            stats.BizonKills++;
                            _logger.LogStatsUpdate(attackerName, "Bizon Kills", stats.BizonKills);
                            break;
                        case "negev":
                            stats.NegevKills++;
                            _logger.LogStatsUpdate(attackerName, "Negev Kills", stats.NegevKills);
                            break;
                        case "m249":
                            stats.M249Kills++;
                            _logger.LogStatsUpdate(attackerName, "M249 Kills", stats.M249Kills);
                            break;
                        // ... más armas según sea necesario
                    }
                }
                catch (Exception ex)
                {
                    _internalLogger?.LogError($"Error updating attacker stats: {ex.Message}");
                }
            });
        }

        private void UpdateAttackerStatsWithHitGroup(CCSPlayerController attacker, string weaponName, int hitGroup)
        {
            // Verificar que el jugador sea válido antes de encolar la operación
            if (attacker == null || !attacker.IsValid)
            {
                return;
            }
            
            // Obtener los datos necesarios mientras el jugador aún es válido
            string attackerName = attacker.PlayerName;
            string steamId = attacker.SteamID.ToString();
            
            // Enqueue the operation to run on the main thread
            MainThreadDispatcher.Enqueue(() => {
                try
                {
                    // Verificar si ya tenemos en caché
                    PlayerStats? stats = null;
                    if (_cachedStats.ContainsKey(steamId))
                    {
                        stats = _cachedStats[steamId];
                    }
                    
                    // Si no tenemos stats en caché, crear nuevas estadísticas
                    if (stats == null)
                    {
                        stats = new PlayerStats
                        {
                            SteamId = steamId,
                            PlayerName = attackerName,
                            LastUpdated = DateTime.UtcNow
                        };
                        _cachedStats[steamId] = stats;
                    }
                    
                    stats.TotalKills++;
                    _logger.LogStatsUpdate(attackerName, "Total Kills", stats.TotalKills);
                    
                    // Determinar el tipo de hit basado en el hitGroup
                    switch (hitGroup)
                    {
                        case 1: // Head
                            stats.TotalHeadshots++;
                            _logger.LogStatsUpdate(attackerName, "Total Headshots", stats.TotalHeadshots);
                            break;
                    }
                    
                    // Convertir hitGroup a enum para mejor legibilidad
                    var hitGroupEnum = (HitGroup)hitGroup;
                    
                    // Incrementar estadísticas específicas por arma y hitgroup
                    switch (weaponName.ToLower())
                    {
                        case "ak47":
                            stats.AK47Kills++;
                            _logger.LogStatsUpdate(attackerName, "AK47 Kills", stats.AK47Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.AK47Headshots++; _logger.LogStatsUpdate(attackerName, "AK47 Headshots", stats.AK47Headshots); break;
                                case HitGroup.Chest: stats.AK47Chestshots++; _logger.LogStatsUpdate(attackerName, "AK47 Chestshots", stats.AK47Chestshots); break;
                                case HitGroup.Stomach: stats.AK47Stomachshots++; _logger.LogStatsUpdate(attackerName, "AK47 Stomachshots", stats.AK47Stomachshots); break;
                                case HitGroup.LeftLeg: stats.AK47LeftLegshots++; _logger.LogStatsUpdate(attackerName, "AK47 Left Legshots", stats.AK47LeftLegshots); break;
                                case HitGroup.RightLeg: stats.AK47RightLegshots++; _logger.LogStatsUpdate(attackerName, "AK47 Right Legshots", stats.AK47RightLegshots); break;
                            }
                            break;
                        case "m4a4":
                            stats.M4A4Kills++;
                            _logger.LogStatsUpdate(attackerName, "M4A4 Kills", stats.M4A4Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.M4A4Headshots++; _logger.LogStatsUpdate(attackerName, "M4A4 Headshots", stats.M4A4Headshots); break;
                                case HitGroup.Chest: stats.M4A4Chestshots++; _logger.LogStatsUpdate(attackerName, "M4A4 Chestshots", stats.M4A4Chestshots); break;
                                case HitGroup.Stomach: stats.M4A4Stomachshots++; _logger.LogStatsUpdate(attackerName, "M4A4 Stomachshots", stats.M4A4Stomachshots); break;
                                case HitGroup.LeftLeg: stats.M4A4LeftLegshots++; _logger.LogStatsUpdate(attackerName, "M4A4 Left Legshots", stats.M4A4LeftLegshots); break;
                                case HitGroup.RightLeg: stats.M4A4RightLegshots++; _logger.LogStatsUpdate(attackerName, "M4A4 Right Legshots", stats.M4A4RightLegshots); break;
                            }
                            break;
                        case "m4a1_silencer":
                        case "m4a1_silencer_off":
                            stats.M4A1SKills++;
                            _logger.LogStatsUpdate(attackerName, "M4A1S Kills", stats.M4A1SKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.M4A1SHeadshots++; _logger.LogStatsUpdate(attackerName, "M4A1S Headshots", stats.M4A1SHeadshots); break;
                                case HitGroup.Chest: stats.M4A1SChestshots++; _logger.LogStatsUpdate(attackerName, "M4A1S Chestshots", stats.M4A1SChestshots); break;
                                case HitGroup.Stomach: stats.M4A1SStomachshots++; _logger.LogStatsUpdate(attackerName, "M4A1S Stomachshots", stats.M4A1SStomachshots); break;
                                case HitGroup.LeftLeg: stats.M4A1SLeftLegshots++; _logger.LogStatsUpdate(attackerName, "M4A1S Left Legshots", stats.M4A1SLeftLegshots); break;
                                case HitGroup.RightLeg: stats.M4A1SRightLegshots++; _logger.LogStatsUpdate(attackerName, "M4A1S Right Legshots", stats.M4A1SRightLegshots); break;
                            }
                            break;
                        case "aug":
                            stats.AUGKills++;
                            _logger.LogStatsUpdate(attackerName, "AUG Kills", stats.AUGKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.AUGHeadshots++; _logger.LogStatsUpdate(attackerName, "AUG Headshots", stats.AUGHeadshots); break;
                                case HitGroup.Chest: stats.AUGChestshots++; _logger.LogStatsUpdate(attackerName, "AUG Chestshots", stats.AUGChestshots); break;
                                case HitGroup.Stomach: stats.AUGStomachshots++; _logger.LogStatsUpdate(attackerName, "AUG Stomachshots", stats.AUGStomachshots); break;
                                case HitGroup.LeftLeg: stats.AUGLeftLegshots++; _logger.LogStatsUpdate(attackerName, "AUG Left Legshots", stats.AUGLeftLegshots); break;
                                case HitGroup.RightLeg: stats.AUGRightLegshots++; _logger.LogStatsUpdate(attackerName, "AUG Right Legshots", stats.AUGRightLegshots); break;
                            }
                            break;
                        case "sg556":
                            stats.SG553Kills++;
                            _logger.LogStatsUpdate(attackerName, "SG553 Kills", stats.SG553Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.SG553Headshots++; _logger.LogStatsUpdate(attackerName, "SG553 Headshots", stats.SG553Headshots); break;
                                case HitGroup.Chest: stats.SG553Chestshots++; _logger.LogStatsUpdate(attackerName, "SG553 Chestshots", stats.SG553Chestshots); break;
                                case HitGroup.Stomach: stats.SG553Stomachshots++; _logger.LogStatsUpdate(attackerName, "SG553 Stomachshots", stats.SG553Stomachshots); break;
                                case HitGroup.LeftLeg: stats.SG553LeftLegshots++; _logger.LogStatsUpdate(attackerName, "SG553 Left Legshots", stats.SG553LeftLegshots); break;
                                case HitGroup.RightLeg: stats.SG553RightLegshots++; _logger.LogStatsUpdate(attackerName, "SG553 Right Legshots", stats.SG553RightLegshots); break;
                            }
                            break;
                        case "galilar":
                            stats.GalilARKills++;
                            _logger.LogStatsUpdate(attackerName, "GalilAR Kills", stats.GalilARKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.GalilARHeadshots++; _logger.LogStatsUpdate(attackerName, "GalilAR Headshots", stats.GalilARHeadshots); break;
                                case HitGroup.Chest: stats.GalilARChestshots++; _logger.LogStatsUpdate(attackerName, "GalilAR Chestshots", stats.GalilARChestshots); break;
                                case HitGroup.Stomach: stats.GalilARStomachshots++; _logger.LogStatsUpdate(attackerName, "GalilAR Stomachshots", stats.GalilARStomachshots); break;
                                case HitGroup.LeftLeg: stats.GalilARLeftLegshots++; _logger.LogStatsUpdate(attackerName, "GalilAR Left Legshots", stats.GalilARLeftLegshots); break;
                                case HitGroup.RightLeg: stats.GalilARRightLegshots++; _logger.LogStatsUpdate(attackerName, "GalilAR Right Legshots", stats.GalilARRightLegshots); break;
                            }
                            break;
                        case "famas":
                            stats.FAMASKills++;
                            _logger.LogStatsUpdate(attackerName, "FAMAS Kills", stats.FAMASKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.FAMASHeadshots++; _logger.LogStatsUpdate(attackerName, "FAMAS Headshots", stats.FAMASHeadshots); break;
                                case HitGroup.Chest: stats.FAMASChestshots++; _logger.LogStatsUpdate(attackerName, "FAMAS Chestshots", stats.FAMASChestshots); break;
                                case HitGroup.Stomach: stats.FAMASStomachshots++; _logger.LogStatsUpdate(attackerName, "FAMAS Stomachshots", stats.FAMASStomachshots); break;
                                case HitGroup.LeftLeg: stats.FAMASLeftLegshots++; _logger.LogStatsUpdate(attackerName, "FAMAS Left Legshots", stats.FAMASLeftLegshots); break;
                                case HitGroup.RightLeg: stats.FAMASRightLegshots++; _logger.LogStatsUpdate(attackerName, "FAMAS Right Legshots", stats.FAMASRightLegshots); break;
                            }
                            break;
                        case "awp":
                            stats.AWPKills++;
                            _logger.LogStatsUpdate(attackerName, "AWP Kills", stats.AWPKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.AWPHeadshots++; _logger.LogStatsUpdate(attackerName, "AWP Headshots", stats.AWPHeadshots); break;
                                case HitGroup.Chest: stats.AWPChestshots++; _logger.LogStatsUpdate(attackerName, "AWP Chestshots", stats.AWPChestshots); break;
                                case HitGroup.Stomach: stats.AWPStomachshots++; _logger.LogStatsUpdate(attackerName, "AWP Stomachshots", stats.AWPStomachshots); break;
                                case HitGroup.LeftLeg: stats.AWPLeftLegshots++; _logger.LogStatsUpdate(attackerName, "AWP Left Legshots", stats.AWPLeftLegshots); break;
                                case HitGroup.RightLeg: stats.AWPRightLegshots++; _logger.LogStatsUpdate(attackerName, "AWP Right Legshots", stats.AWPRightLegshots); break;
                            }
                            break;
                        case "ssg08":
                            stats.SSG08Kills++;
                            _logger.LogStatsUpdate(attackerName, "SSG08 Kills", stats.SSG08Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.SSG08Headshots++; _logger.LogStatsUpdate(attackerName, "SSG08 Headshots", stats.SSG08Headshots); break;
                                case HitGroup.Chest: stats.SSG08Chestshots++; _logger.LogStatsUpdate(attackerName, "SSG08 Chestshots", stats.SSG08Chestshots); break;
                                case HitGroup.Stomach: stats.SSG08Stomachshots++; _logger.LogStatsUpdate(attackerName, "SSG08 Stomachshots", stats.SSG08Stomachshots); break;
                                case HitGroup.LeftLeg: stats.SSG08LeftLegshots++; _logger.LogStatsUpdate(attackerName, "SSG08 Left Legshots", stats.SSG08LeftLegshots); break;
                                case HitGroup.RightLeg: stats.SSG08RightLegshots++; _logger.LogStatsUpdate(attackerName, "SSG08 Right Legshots", stats.SSG08RightLegshots); break;
                            }
                            break;
                        case "scar20":
                            stats.Scar20Kills++;
                            _logger.LogStatsUpdate(attackerName, "Scar20 Kills", stats.Scar20Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.Scar20Headshots++; _logger.LogStatsUpdate(attackerName, "Scar20 Headshots", stats.Scar20Headshots); break;
                                case HitGroup.Chest: stats.Scar20Chestshots++; _logger.LogStatsUpdate(attackerName, "Scar20 Chestshots", stats.Scar20Chestshots); break;
                                case HitGroup.Stomach: stats.Scar20Stomachshots++; _logger.LogStatsUpdate(attackerName, "Scar20 Stomachshots", stats.Scar20Stomachshots); break;
                                case HitGroup.LeftLeg: stats.Scar20LeftLegshots++; _logger.LogStatsUpdate(attackerName, "Scar20 Left Legshots", stats.Scar20LeftLegshots); break;
                                case HitGroup.RightLeg: stats.Scar20RightLegshots++; _logger.LogStatsUpdate(attackerName, "Scar20 Right Legshots", stats.Scar20RightLegshots); break;
                            }
                            break;
                        case "g3sg1":
                            stats.G3SG1Kills++;
                            _logger.LogStatsUpdate(attackerName, "G3SG1 Kills", stats.G3SG1Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.G3SG1Headshots++; _logger.LogStatsUpdate(attackerName, "G3SG1 Headshots", stats.G3SG1Headshots); break;
                                case HitGroup.Chest: stats.G3SG1Chestshots++; _logger.LogStatsUpdate(attackerName, "G3SG1 Chestshots", stats.G3SG1Chestshots); break;
                                case HitGroup.Stomach: stats.G3SG1Stomachshots++; _logger.LogStatsUpdate(attackerName, "G3SG1 Stomachshots", stats.G3SG1Stomachshots); break;
                                case HitGroup.LeftLeg: stats.G3SG1LeftLegshots++; _logger.LogStatsUpdate(attackerName, "G3SG1 Left Legshots", stats.G3SG1LeftLegshots); break;
                                case HitGroup.RightLeg: stats.G3SG1RightLegshots++; _logger.LogStatsUpdate(attackerName, "G3SG1 Right Legshots", stats.G3SG1RightLegshots); break;
                            }
                            break;
                        case "deagle":
                            stats.DeagleKills++;
                            _logger.LogStatsUpdate(attackerName, "Deagle Kills", stats.DeagleKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.DeagleHeadshots++; _logger.LogStatsUpdate(attackerName, "Deagle Headshots", stats.DeagleHeadshots); break;
                                case HitGroup.Chest: stats.DeagleChestshots++; _logger.LogStatsUpdate(attackerName, "Deagle Chestshots", stats.DeagleChestshots); break;
                                case HitGroup.Stomach: stats.DeagleStomachshots++; _logger.LogStatsUpdate(attackerName, "Deagle Stomachshots", stats.DeagleStomachshots); break;
                                case HitGroup.LeftLeg: stats.DeagleLeftLegshots++; _logger.LogStatsUpdate(attackerName, "Deagle Left Legshots", stats.DeagleLeftLegshots); break;
                                case HitGroup.RightLeg: stats.DeagleRightLegshots++; _logger.LogStatsUpdate(attackerName, "Deagle Right Legshots", stats.DeagleRightLegshots); break;
                            }
                            break;
                        case "glock":
                            stats.GlockKills++;
                            _logger.LogStatsUpdate(attackerName, "Glock Kills", stats.GlockKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.GlockHeadshots++; _logger.LogStatsUpdate(attackerName, "Glock Headshots", stats.GlockHeadshots); break;
                                case HitGroup.Chest: stats.GlockChestshots++; _logger.LogStatsUpdate(attackerName, "Glock Chestshots", stats.GlockChestshots); break;
                                case HitGroup.Stomach: stats.GlockStomachshots++; _logger.LogStatsUpdate(attackerName, "Glock Stomachshots", stats.GlockStomachshots); break;
                                case HitGroup.LeftLeg: stats.GlockLeftLegshots++; _logger.LogStatsUpdate(attackerName, "Glock Left Legshots", stats.GlockLeftLegshots); break;
                                case HitGroup.RightLeg: stats.GlockRightLegshots++; _logger.LogStatsUpdate(attackerName, "Glock Right Legshots", stats.GlockRightLegshots); break;
                            }
                            break;
                        case "usp_silencer":
                        case "usp_silencer_off":
                            stats.USPSKills++;
                            _logger.LogStatsUpdate(attackerName, "USP-S Kills", stats.USPSKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.USPSHeadshots++; _logger.LogStatsUpdate(attackerName, "USP-S Headshots", stats.USPSHeadshots); break;
                                case HitGroup.Chest: stats.USPSChestshots++; _logger.LogStatsUpdate(attackerName, "USP-S Chestshots", stats.USPSChestshots); break;
                                case HitGroup.Stomach: stats.USPSStomachshots++; _logger.LogStatsUpdate(attackerName, "USP-S Stomachshots", stats.USPSStomachshots); break;
                                case HitGroup.LeftLeg: stats.USPSLeftLegshots++; _logger.LogStatsUpdate(attackerName, "USP-S Left Legshots", stats.USPSLeftLegshots); break;
                                case HitGroup.RightLeg: stats.USPSRightLegshots++; _logger.LogStatsUpdate(attackerName, "USP-S Right Legshots", stats.USPSRightLegshots); break;
                            }
                            break;
                        case "p250":
                            stats.P250Kills++;
                            _logger.LogStatsUpdate(attackerName, "P250 Kills", stats.P250Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.P250Headshots++; _logger.LogStatsUpdate(attackerName, "P250 Headshots", stats.P250Headshots); break;
                                case HitGroup.Chest: stats.P250Chestshots++; _logger.LogStatsUpdate(attackerName, "P250 Chestshots", stats.P250Chestshots); break;
                                case HitGroup.Stomach: stats.P250Stomachshots++; _logger.LogStatsUpdate(attackerName, "P250 Stomachshots", stats.P250Stomachshots); break;
                                case HitGroup.LeftLeg: stats.P250LeftLegshots++; _logger.LogStatsUpdate(attackerName, "P250 Left Legshots", stats.P250LeftLegshots); break;
                                case HitGroup.RightLeg: stats.P250RightLegshots++; _logger.LogStatsUpdate(attackerName, "P250 Right Legshots", stats.P250RightLegshots); break;
                            }
                            break;
                        case "hkp2000":
                            stats.P2000Kills++;
                            _logger.LogStatsUpdate(attackerName, "P2000 Kills", stats.P2000Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.P2000Headshots++; _logger.LogStatsUpdate(attackerName, "P2000 Headshots", stats.P2000Headshots); break;
                                case HitGroup.Chest: stats.P2000Chestshots++; _logger.LogStatsUpdate(attackerName, "P2000 Chestshots", stats.P2000Chestshots); break;
                                case HitGroup.Stomach: stats.P2000Stomachshots++; _logger.LogStatsUpdate(attackerName, "P2000 Stomachshots", stats.P2000Stomachshots); break;
                                case HitGroup.LeftLeg: stats.P2000LeftLegshots++; _logger.LogStatsUpdate(attackerName, "P2000 Left Legshots", stats.P2000LeftLegshots); break;
                                case HitGroup.RightLeg: stats.P2000RightLegshots++; _logger.LogStatsUpdate(attackerName, "P2000 Right Legshots", stats.P2000RightLegshots); break;
                            }
                            break;
                        case "fiveseven":
                            stats.FiveSevenKills++;
                            _logger.LogStatsUpdate(attackerName, "FiveSeven Kills", stats.FiveSevenKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.FiveSevenHeadshots++; _logger.LogStatsUpdate(attackerName, "FiveSeven Headshots", stats.FiveSevenHeadshots); break;
                                case HitGroup.Chest: stats.FiveSevenChestshots++; _logger.LogStatsUpdate(attackerName, "FiveSeven Chestshots", stats.FiveSevenChestshots); break;
                                case HitGroup.Stomach: stats.FiveSevenStomachshots++; _logger.LogStatsUpdate(attackerName, "FiveSeven Stomachshots", stats.FiveSevenStomachshots); break;
                                case HitGroup.LeftLeg: stats.FiveSevenLeftLegshots++; _logger.LogStatsUpdate(attackerName, "FiveSeven Left Legshots", stats.FiveSevenLeftLegshots); break;
                                case HitGroup.RightLeg: stats.FiveSevenRightLegshots++; _logger.LogStatsUpdate(attackerName, "FiveSeven Right Legshots", stats.FiveSevenRightLegshots); break;
                            }
                            break;
                        case "tec9":
                            stats.Tec9Kills++;
                            _logger.LogStatsUpdate(attackerName, "Tec9 Kills", stats.Tec9Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.Tec9Headshots++; _logger.LogStatsUpdate(attackerName, "Tec9 Headshots", stats.Tec9Headshots); break;
                                case HitGroup.Chest: stats.Tec9Chestshots++; _logger.LogStatsUpdate(attackerName, "Tec9 Chestshots", stats.Tec9Chestshots); break;
                                case HitGroup.Stomach: stats.Tec9Stomachshots++; _logger.LogStatsUpdate(attackerName, "Tec9 Stomachshots", stats.Tec9Stomachshots); break;
                                case HitGroup.LeftLeg: stats.Tec9LeftLegshots++; _logger.LogStatsUpdate(attackerName, "Tec9 Left Legshots", stats.Tec9LeftLegshots); break;
                                case HitGroup.RightLeg: stats.Tec9RightLegshots++; _logger.LogStatsUpdate(attackerName, "Tec9 Right Legshots", stats.Tec9RightLegshots); break;
                            }
                            break;
                        case "cz75a":
                            stats.CZ75Kills++;
                            _logger.LogStatsUpdate(attackerName, "CZ75 Kills", stats.CZ75Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.CZ75Headshots++; _logger.LogStatsUpdate(attackerName, "CZ75 Headshots", stats.CZ75Headshots); break;
                                case HitGroup.Chest: stats.CZ75Chestshots++; _logger.LogStatsUpdate(attackerName, "CZ75 Chestshots", stats.CZ75Chestshots); break;
                                case HitGroup.Stomach: stats.CZ75Stomachshots++; _logger.LogStatsUpdate(attackerName, "CZ75 Stomachshots", stats.CZ75Stomachshots); break;
                                case HitGroup.LeftLeg: stats.CZ75LeftLegshots++; _logger.LogStatsUpdate(attackerName, "CZ75 Left Legshots", stats.CZ75LeftLegshots); break;
                                case HitGroup.RightLeg: stats.CZ75RightLegshots++; _logger.LogStatsUpdate(attackerName, "CZ75 Right Legshots", stats.CZ75RightLegshots); break;
                            }
                            break;
                        case "elite":
                            stats.DualiesKills++;
                            _logger.LogStatsUpdate(attackerName, "Dualies Kills", stats.DualiesKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.DualiesHeadshots++; _logger.LogStatsUpdate(attackerName, "Dualies Headshots", stats.DualiesHeadshots); break;
                                case HitGroup.Chest: stats.DualiesChestshots++; _logger.LogStatsUpdate(attackerName, "Dualies Chestshots", stats.DualiesChestshots); break;
                                case HitGroup.Stomach: stats.DualiesStomachshots++; _logger.LogStatsUpdate(attackerName, "Dualies Stomachshots", stats.DualiesStomachshots); break;
                                case HitGroup.LeftLeg: stats.DualiesLeftLegshots++; _logger.LogStatsUpdate(attackerName, "Dualies Left Legshots", stats.DualiesLeftLegshots); break;
                                case HitGroup.RightLeg: stats.DualiesRightLegshots++; _logger.LogStatsUpdate(attackerName, "Dualies Right Legshots", stats.DualiesRightLegshots); break;
                            }
                            break;
                        case "revolver":
                            stats.RevolverKills++;
                            _logger.LogStatsUpdate(attackerName, "Revolver Kills", stats.RevolverKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.RevolverHeadshots++; _logger.LogStatsUpdate(attackerName, "Revolver Headshots", stats.RevolverHeadshots); break;
                                case HitGroup.Chest: stats.RevolverChestshots++; _logger.LogStatsUpdate(attackerName, "Revolver Chestshots", stats.RevolverChestshots); break;
                                case HitGroup.Stomach: stats.RevolverStomachshots++; _logger.LogStatsUpdate(attackerName, "Revolver Stomachshots", stats.RevolverStomachshots); break;
                                case HitGroup.LeftLeg: stats.RevolverLeftLegshots++; _logger.LogStatsUpdate(attackerName, "Revolver Left Legshots", stats.RevolverLeftLegshots); break;
                                case HitGroup.RightLeg: stats.RevolverRightLegshots++; _logger.LogStatsUpdate(attackerName, "Revolver Right Legshots", stats.RevolverRightLegshots); break;
                            }
                            break;
                        case "nova":
                            stats.NovaKills++;
                            _logger.LogStatsUpdate(attackerName, "Nova Kills", stats.NovaKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.NovaHeadshots++; _logger.LogStatsUpdate(attackerName, "Nova Headshots", stats.NovaHeadshots); break;
                                case HitGroup.Chest: stats.NovaChestshots++; _logger.LogStatsUpdate(attackerName, "Nova Chestshots", stats.NovaChestshots); break;
                                case HitGroup.Stomach: stats.NovaStomachshots++; _logger.LogStatsUpdate(attackerName, "Nova Stomachshots", stats.NovaStomachshots); break;
                                case HitGroup.LeftLeg: stats.NovaLeftLegshots++; _logger.LogStatsUpdate(attackerName, "Nova Left Legshots", stats.NovaLeftLegshots); break;
                                case HitGroup.RightLeg: stats.NovaRightLegshots++; _logger.LogStatsUpdate(attackerName, "Nova Right Legshots", stats.NovaRightLegshots); break;
                            }
                            break;
                        case "xm1014":
                            stats.XM1014Kills++;
                            _logger.LogStatsUpdate(attackerName, "XM1014 Kills", stats.XM1014Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.XM1014Headshots++; _logger.LogStatsUpdate(attackerName, "XM1014 Headshots", stats.XM1014Headshots); break;
                                case HitGroup.Chest: stats.XM1014Chestshots++; _logger.LogStatsUpdate(attackerName, "XM1014 Chestshots", stats.XM1014Chestshots); break;
                                case HitGroup.Stomach: stats.XM1014Stomachshots++; _logger.LogStatsUpdate(attackerName, "XM1014 Stomachshots", stats.XM1014Stomachshots); break;
                                case HitGroup.LeftLeg: stats.XM1014LeftLegshots++; _logger.LogStatsUpdate(attackerName, "XM1014 Left Legshots", stats.XM1014LeftLegshots); break;
                                case HitGroup.RightLeg: stats.XM1014RightLegshots++; _logger.LogStatsUpdate(attackerName, "XM1014 Right Legshots", stats.XM1014RightLegshots); break;
                            }
                            break;
                        case "sawedoff":
                            stats.SawedOffKills++;
                            _logger.LogStatsUpdate(attackerName, "SawedOff Kills", stats.SawedOffKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.SawedOffHeadshots++; _logger.LogStatsUpdate(attackerName, "SawedOff Headshots", stats.SawedOffHeadshots); break;
                                case HitGroup.Chest: stats.SawedOffChestshots++; _logger.LogStatsUpdate(attackerName, "SawedOff Chestshots", stats.SawedOffChestshots); break;
                                case HitGroup.Stomach: stats.SawedOffStomachshots++; _logger.LogStatsUpdate(attackerName, "SawedOff Stomachshots", stats.SawedOffStomachshots); break;
                                case HitGroup.LeftLeg: stats.SawedOffLeftLegshots++; _logger.LogStatsUpdate(attackerName, "SawedOff Left Legshots", stats.SawedOffLeftLegshots); break;
                                case HitGroup.RightLeg: stats.SawedOffRightLegshots++; _logger.LogStatsUpdate(attackerName, "SawedOff Right Legshots", stats.SawedOffRightLegshots); break;
                            }
                            break;
                        case "mag7":
                            stats.MAG7Kills++;
                            _logger.LogStatsUpdate(attackerName, "MAG7 Kills", stats.MAG7Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.MAG7Headshots++; _logger.LogStatsUpdate(attackerName, "MAG7 Headshots", stats.MAG7Headshots); break;
                                case HitGroup.Chest: stats.MAG7Chestshots++; _logger.LogStatsUpdate(attackerName, "MAG7 Chestshots", stats.MAG7Chestshots); break;
                                case HitGroup.Stomach: stats.MAG7Stomachshots++; _logger.LogStatsUpdate(attackerName, "MAG7 Stomachshots", stats.MAG7Stomachshots); break;
                                case HitGroup.LeftLeg: stats.MAG7LeftLegshots++; _logger.LogStatsUpdate(attackerName, "MAG7 Left Legshots", stats.MAG7LeftLegshots); break;
                                case HitGroup.RightLeg: stats.MAG7RightLegshots++; _logger.LogStatsUpdate(attackerName, "MAG7 Right Legshots", stats.MAG7RightLegshots); break;
                            }
                            break;
                        case "mac10":
                            stats.Mac10Kills++;
                            _logger.LogStatsUpdate(attackerName, "Mac10 Kills", stats.Mac10Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.Mac10Headshots++; _logger.LogStatsUpdate(attackerName, "Mac10 Headshots", stats.Mac10Headshots); break;
                                case HitGroup.Chest: stats.Mac10Chestshots++; _logger.LogStatsUpdate(attackerName, "Mac10 Chestshots", stats.Mac10Chestshots); break;
                                case HitGroup.Stomach: stats.Mac10Stomachshots++; _logger.LogStatsUpdate(attackerName, "Mac10 Stomachshots", stats.Mac10Stomachshots); break;
                                case HitGroup.LeftLeg: stats.Mac10LeftLegshots++; _logger.LogStatsUpdate(attackerName, "Mac10 Left Legshots", stats.Mac10LeftLegshots); break;
                                case HitGroup.RightLeg: stats.Mac10RightLegshots++; _logger.LogStatsUpdate(attackerName, "Mac10 Right Legshots", stats.Mac10RightLegshots); break;
                            }
                            break;
                        case "mp9":
                            stats.MP9Kills++;
                            _logger.LogStatsUpdate(attackerName, "MP9 Kills", stats.MP9Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.MP9Headshots++; _logger.LogStatsUpdate(attackerName, "MP9 Headshots", stats.MP9Headshots); break;
                                case HitGroup.Chest: stats.MP9Chestshots++; _logger.LogStatsUpdate(attackerName, "MP9 Chestshots", stats.MP9Chestshots); break;
                                case HitGroup.Stomach: stats.MP9Stomachshots++; _logger.LogStatsUpdate(attackerName, "MP9 Stomachshots", stats.MP9Stomachshots); break;
                                case HitGroup.LeftLeg: stats.MP9LeftLegshots++; _logger.LogStatsUpdate(attackerName, "MP9 Left Legshots", stats.MP9LeftLegshots); break;
                                case HitGroup.RightLeg: stats.MP9RightLegshots++; _logger.LogStatsUpdate(attackerName, "MP9 Right Legshots", stats.MP9RightLegshots); break;
                            }
                            break;
                        case "mp7":
                            stats.MP7Kills++;
                            _logger.LogStatsUpdate(attackerName, "MP7 Kills", stats.MP7Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.MP7Headshots++; _logger.LogStatsUpdate(attackerName, "MP7 Headshots", stats.MP7Headshots); break;
                                case HitGroup.Chest: stats.MP7Chestshots++; _logger.LogStatsUpdate(attackerName, "MP7 Chestshots", stats.MP7Chestshots); break;
                                case HitGroup.Stomach: stats.MP7Stomachshots++; _logger.LogStatsUpdate(attackerName, "MP7 Stomachshots", stats.MP7Stomachshots); break;
                                case HitGroup.LeftLeg: stats.MP7LeftLegshots++; _logger.LogStatsUpdate(attackerName, "MP7 Left Legshots", stats.MP7LeftLegshots); break;
                                case HitGroup.RightLeg: stats.MP7RightLegshots++; _logger.LogStatsUpdate(attackerName, "MP7 Right Legshots", stats.MP7RightLegshots); break;
                            }
                            break;
                        case "ump45":
                            stats.UMP45Kills++;
                            _logger.LogStatsUpdate(attackerName, "UMP45 Kills", stats.UMP45Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.UMP45Headshots++; _logger.LogStatsUpdate(attackerName, "UMP45 Headshots", stats.UMP45Headshots); break;
                                case HitGroup.Chest: stats.UMP45Chestshots++; _logger.LogStatsUpdate(attackerName, "UMP45 Chestshots", stats.UMP45Chestshots); break;
                                case HitGroup.Stomach: stats.UMP45Stomachshots++; _logger.LogStatsUpdate(attackerName, "UMP45 Stomachshots", stats.UMP45Stomachshots); break;
                                case HitGroup.LeftLeg: stats.UMP45LeftLegshots++; _logger.LogStatsUpdate(attackerName, "UMP45 Left Legshots", stats.UMP45LeftLegshots); break;
                                case HitGroup.RightLeg: stats.UMP45RightLegshots++; _logger.LogStatsUpdate(attackerName, "UMP45 Right Legshots", stats.UMP45RightLegshots); break;
                            }
                            break;
                        case "p90":
                            stats.P90Kills++;
                            _logger.LogStatsUpdate(attackerName, "P90 Kills", stats.P90Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.P90Headshots++; _logger.LogStatsUpdate(attackerName, "P90 Headshots", stats.P90Headshots); break;
                                case HitGroup.Chest: stats.P90Chestshots++; _logger.LogStatsUpdate(attackerName, "P90 Chestshots", stats.P90Chestshots); break;
                                case HitGroup.Stomach: stats.P90Stomachshots++; _logger.LogStatsUpdate(attackerName, "P90 Stomachshots", stats.P90Stomachshots); break;
                                case HitGroup.LeftLeg: stats.P90LeftLegshots++; _logger.LogStatsUpdate(attackerName, "P90 Left Legshots", stats.P90LeftLegshots); break;
                                case HitGroup.RightLeg: stats.P90RightLegshots++; _logger.LogStatsUpdate(attackerName, "P90 Right Legshots", stats.P90RightLegshots); break;
                            }
                            break;
                        case "bizon":
                            stats.BizonKills++;
                            _logger.LogStatsUpdate(attackerName, "Bizon Kills", stats.BizonKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.BizonHeadshots++; _logger.LogStatsUpdate(attackerName, "Bizon Headshots", stats.BizonHeadshots); break;
                                case HitGroup.Chest: stats.BizonChestshots++; _logger.LogStatsUpdate(attackerName, "Bizon Chestshots", stats.BizonChestshots); break;
                                case HitGroup.Stomach: stats.BizonStomachshots++; _logger.LogStatsUpdate(attackerName, "Bizon Stomachshots", stats.BizonStomachshots); break;
                                case HitGroup.LeftLeg: stats.BizonLeftLegshots++; _logger.LogStatsUpdate(attackerName, "Bizon Left Legshots", stats.BizonLeftLegshots); break;
                                case HitGroup.RightLeg: stats.BizonRightLegshots++; _logger.LogStatsUpdate(attackerName, "Bizon Right Legshots", stats.BizonRightLegshots); break;
                            }
                            break;
                        case "negev":
                            stats.NegevKills++;
                            _logger.LogStatsUpdate(attackerName, "Negev Kills", stats.NegevKills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.NegevHeadshots++; _logger.LogStatsUpdate(attackerName, "Negev Headshots", stats.NegevHeadshots); break;
                                case HitGroup.Chest: stats.NegevChestshots++; _logger.LogStatsUpdate(attackerName, "Negev Chestshots", stats.NegevChestshots); break;
                                case HitGroup.Stomach: stats.NegevStomachshots++; _logger.LogStatsUpdate(attackerName, "Negev Stomachshots", stats.NegevStomachshots); break;
                                case HitGroup.LeftLeg: stats.NegevLeftLegshots++; _logger.LogStatsUpdate(attackerName, "Negev Left Legshots", stats.NegevLeftLegshots); break;
                                case HitGroup.RightLeg: stats.NegevRightLegshots++; _logger.LogStatsUpdate(attackerName, "Negev Right Legshots", stats.NegevRightLegshots); break;
                            }
                            break;
                        case "m249":
                            stats.M249Kills++;
                            _logger.LogStatsUpdate(attackerName, "M249 Kills", stats.M249Kills);
                            switch (hitGroupEnum)
                            {
                                case HitGroup.Head: stats.M249Headshots++; _logger.LogStatsUpdate(attackerName, "M249 Headshots", stats.M249Headshots); break;
                                case HitGroup.Chest: stats.M249Chestshots++; _logger.LogStatsUpdate(attackerName, "M249 Chestshots", stats.M249Chestshots); break;
                                case HitGroup.Stomach: stats.M249Stomachshots++; _logger.LogStatsUpdate(attackerName, "M249 Stomachshots", stats.M249Stomachshots); break;
                                case HitGroup.LeftLeg: stats.M249LeftLegshots++; _logger.LogStatsUpdate(attackerName, "M249 Left Legshots", stats.M249LeftLegshots); break;
                                case HitGroup.RightLeg: stats.M249RightLegshots++; _logger.LogStatsUpdate(attackerName, "M249 Right Legshots", stats.M249RightLegshots); break;
                            }
                            break;
                        // ... más armas según sea necesario
                    }
                }
                catch (Exception ex)
                {
                    _internalLogger?.LogError($"Error updating attacker stats with hitgroup: {ex.Message}");
                }
            });
        }

        private void UpdateVictimStats(CCSPlayerController victim, string weaponName, string attackerName)
        {
            // Verificar que el jugador sea válido antes de encolar la operación
            if (victim == null || !victim.IsValid)
            {
                return;
            }
            
            // Obtener los datos necesarios mientras el jugador aún es válido
            string victimName = victim.PlayerName;
            string steamId = victim.SteamID.ToString();
            
            // Enqueue the operation to run on the main thread
            MainThreadDispatcher.Enqueue(() => {
                try
                {
                    // Verificar si ya tenemos en caché
                    PlayerStats? stats = null;
                    if (_cachedStats.ContainsKey(steamId))
                    {
                        stats = _cachedStats[steamId];
                    }
                    
                    // Si no tenemos stats en caché, crear nuevas estadísticas
                    if (stats == null)
                    {
                        stats = new PlayerStats
                        {
                            SteamId = steamId,
                            PlayerName = victimName,
                            LastUpdated = DateTime.UtcNow
                        };
                        _cachedStats[steamId] = stats;
                    }
                    
                    stats.TotalDeaths++;
                    _logger.LogStatsUpdate(victimName, "Total Deaths", stats.TotalDeaths);
                    
                    // Incrementar muertes específicas por arma
                    switch (weaponName.ToLower())
                    {
                        case "ak47":
                            stats.AK47Deaths++;
                            _logger.LogStatsUpdate(victimName, "AK47 Deaths", stats.AK47Deaths);
                            break;
                        case "m4a4":
                            stats.M4A4Deaths++;
                            _logger.LogStatsUpdate(victimName, "M4A4 Deaths", stats.M4A4Deaths);
                            break;
                        case "m4a1_silencer":
                        case "m4a1_silencer_off":
                            stats.M4A1SDeaths++;
                            _logger.LogStatsUpdate(victimName, "M4A1S Deaths", stats.M4A1SDeaths);
                            break;
                        case "aug":
                            stats.AUGDeaths++;
                            _logger.LogStatsUpdate(victimName, "AUG Deaths", stats.AUGDeaths);
                            break;
                        case "sg556":
                            stats.SG553Deaths++;
                            _logger.LogStatsUpdate(victimName, "SG553 Deaths", stats.SG553Deaths);
                            break;
                        case "galilar":
                            stats.GalilARDeaths++;
                            _logger.LogStatsUpdate(victimName, "GalilAR Deaths", stats.GalilARDeaths);
                            break;
                        case "famas":
                            stats.FAMASDeaths++;
                            _logger.LogStatsUpdate(victimName, "FAMAS Deaths", stats.FAMASDeaths);
                            break;
                        case "awp":
                            stats.AWPDeaths++;
                            _logger.LogStatsUpdate(victimName, "AWP Deaths", stats.AWPDeaths);
                            break;
                        case "ssg08":
                            stats.SSG08Deaths++;
                            _logger.LogStatsUpdate(victimName, "SSG08 Deaths", stats.SSG08Deaths);
                            break;
                        case "scar20":
                            stats.Scar20Deaths++;
                            _logger.LogStatsUpdate(victimName, "Scar20 Deaths", stats.Scar20Deaths);
                            break;
                        case "g3sg1":
                            stats.G3SG1Deaths++;
                            _logger.LogStatsUpdate(victimName, "G3SG1 Deaths", stats.G3SG1Deaths);
                            break;
                        case "deagle":
                            stats.DeagleDeaths++;
                            _logger.LogStatsUpdate(victimName, "Deagle Deaths", stats.DeagleDeaths);
                            break;
                        case "glock":
                            stats.GlockDeaths++;
                            _logger.LogStatsUpdate(victimName, "Glock Deaths", stats.GlockDeaths);
                            break;
                        case "usp_silencer":
                        case "usp_silencer_off":
                            stats.USPSDeaths++;
                            _logger.LogStatsUpdate(victimName, "USP-S Deaths", stats.USPSDeaths);
                            break;
                        case "p250":
                            stats.P250Deaths++;
                            _logger.LogStatsUpdate(victimName, "P250 Deaths", stats.P250Deaths);
                            break;
                        case "hkp2000":
                            stats.P2000Deaths++;
                            _logger.LogStatsUpdate(victimName, "P2000 Deaths", stats.P2000Deaths);
                            break;
                        case "fiveseven":
                            stats.FiveSevenDeaths++;
                            _logger.LogStatsUpdate(victimName, "FiveSeven Deaths", stats.FiveSevenDeaths);
                            break;
                        case "tec9":
                            stats.Tec9Deaths++;
                            _logger.LogStatsUpdate(victimName, "Tec9 Deaths", stats.Tec9Deaths);
                            break;
                        case "cz75a":
                            stats.CZ75Deaths++;
                            _logger.LogStatsUpdate(victimName, "CZ75 Deaths", stats.CZ75Deaths);
                            break;
                        case "elite":
                            stats.DualiesDeaths++;
                            _logger.LogStatsUpdate(victimName, "Dualies Deaths", stats.DualiesDeaths);
                            break;
                        case "revolver":
                            stats.RevolverDeaths++;
                            _logger.LogStatsUpdate(victimName, "Revolver Deaths", stats.RevolverDeaths);
                            break;
                        case "nova":
                            stats.NovaDeaths++;
                            _logger.LogStatsUpdate(victimName, "Nova Deaths", stats.NovaDeaths);
                            break;
                        case "xm1014":
                            stats.XM1014Deaths++;
                            _logger.LogStatsUpdate(victimName, "XM1014 Deaths", stats.XM1014Deaths);
                            break;
                        case "sawedoff":
                            stats.SawedOffDeaths++;
                            _logger.LogStatsUpdate(victimName, "SawedOff Deaths", stats.SawedOffDeaths);
                            break;
                        case "mag7":
                            stats.MAG7Deaths++;
                            _logger.LogStatsUpdate(victimName, "MAG7 Deaths", stats.MAG7Deaths);
                            break;
                        case "mac10":
                            stats.Mac10Deaths++;
                            _logger.LogStatsUpdate(victimName, "Mac10 Deaths", stats.Mac10Deaths);
                            break;
                        case "mp9":
                            stats.MP9Deaths++;
                            _logger.LogStatsUpdate(victimName, "MP9 Deaths", stats.MP9Deaths);
                            break;
                        case "mp7":
                            stats.MP7Deaths++;
                            _logger.LogStatsUpdate(victimName, "MP7 Deaths", stats.MP7Deaths);
                            break;
                        case "ump45":
                            stats.UMP45Deaths++;
                            _logger.LogStatsUpdate(victimName, "UMP45 Deaths", stats.UMP45Deaths);
                            break;
                        case "p90":
                            stats.P90Deaths++;
                            _logger.LogStatsUpdate(victimName, "P90 Deaths", stats.P90Deaths);
                            break;
                        case "bizon":
                            stats.BizonDeaths++;
                            _logger.LogStatsUpdate(victimName, "Bizon Deaths", stats.BizonDeaths);
                            break;
                        case "negev":
                            stats.NegevDeaths++;
                            _logger.LogStatsUpdate(victimName, "Negev Deaths", stats.NegevDeaths);
                            break;
                        case "m249":
                            stats.M249Deaths++;
                            _logger.LogStatsUpdate(victimName, "M249 Deaths", stats.M249Deaths);
                            break;
                        // ... más armas según sea necesario
                    }
                    
                    // Ahora podemos usar el nombre del atacante
                    _logger.LogPlayerDeath(victimName, attackerName, weaponName);
                }
                catch (Exception ex)
                {
                    _internalLogger?.LogError($"Error updating victim stats: {ex.Message}");
                }
            });
        }

        private void UpdateAssistStats(CCSPlayerController assister, string weaponName, string attackerName)
        {
            // Verificar que el jugador sea válido antes de encolar la operación
            if (assister == null || !assister.IsValid)
            {
                return;
            }
            
            // Obtener los datos necesarios mientras el jugador aún es válido
            string assisterName = assister.PlayerName;
            string steamId = assister.SteamID.ToString();
            
            // Enqueue the operation to run on the main thread
            MainThreadDispatcher.Enqueue(() => {
                try
                {
                    // Verificar si ya tenemos en caché
                    PlayerStats? stats = null;
                    if (_cachedStats.ContainsKey(steamId))
                    {
                        stats = _cachedStats[steamId];
                    }
                    
                    // Si no tenemos stats en caché, crear nuevas estadísticas
                    if (stats == null)
                    {
                        stats = new PlayerStats
                        {
                            SteamId = steamId,
                            PlayerName = assisterName,
                            LastUpdated = DateTime.UtcNow
                        };
                        _cachedStats[steamId] = stats;
                    }
                    
                    stats.TotalAssists++;
                    _logger.LogStatsUpdate(assisterName, "Total Assists", stats.TotalAssists);
                    
                    // Incrementar asistencias específicas por arma
                    switch (weaponName.ToLower())
                    {
                        case "ak47":
                            stats.AK47Assists++;
                            _logger.LogStatsUpdate(assisterName, "AK47 Assists", stats.AK47Assists);
                            break;
                        case "m4a4":
                            stats.M4A4Assists++;
                            _logger.LogStatsUpdate(assisterName, "M4A4 Assists", stats.M4A4Assists);
                            break;
                        case "m4a1_silencer":
                        case "m4a1_silencer_off":
                            stats.M4A1SAssists++;
                            _logger.LogStatsUpdate(assisterName, "M4A1S Assists", stats.M4A1SAssists);
                            break;
                        case "aug":
                            stats.AUGAssists++;
                            _logger.LogStatsUpdate(assisterName, "AUG Assists", stats.AUGAssists);
                            break;
                        case "sg556":
                            stats.SG553Assists++;
                            _logger.LogStatsUpdate(assisterName, "SG553 Assists", stats.SG553Assists);
                            break;
                        case "galilar":
                            stats.GalilARAssists++;
                            _logger.LogStatsUpdate(assisterName, "GalilAR Assists", stats.GalilARAssists);
                            break;
                        case "famas":
                            stats.FAMASAssists++;
                            _logger.LogStatsUpdate(assisterName, "FAMAS Assists", stats.FAMASAssists);
                            break;
                        case "awp":
                            stats.AWPAssists++;
                            _logger.LogStatsUpdate(assisterName, "AWP Assists", stats.AWPAssists);
                            break;
                        case "ssg08":
                            stats.SSG08Assists++;
                            _logger.LogStatsUpdate(assisterName, "SSG08 Assists", stats.SSG08Assists);
                            break;
                        case "scar20":
                            stats.Scar20Assists++;
                            _logger.LogStatsUpdate(assisterName, "Scar20 Assists", stats.Scar20Assists);
                            break;
                        case "g3sg1":
                            stats.G3SG1Assists++;
                            _logger.LogStatsUpdate(assisterName, "G3SG1 Assists", stats.G3SG1Assists);
                            break;
                        case "deagle":
                            stats.DeagleAssists++;
                            _logger.LogStatsUpdate(assisterName, "Deagle Assists", stats.DeagleAssists);
                            break;
                        case "glock":
                            stats.GlockAssists++;
                            _logger.LogStatsUpdate(assisterName, "Glock Assists", stats.GlockAssists);
                            break;
                        case "usp_silencer":
                        case "usp_silencer_off":
                            stats.USPSAssists++;
                            _logger.LogStatsUpdate(assisterName, "USP-S Assists", stats.USPSAssists);
                            break;
                        case "p250":
                            stats.P250Assists++;
                            _logger.LogStatsUpdate(assisterName, "P250 Assists", stats.P250Assists);
                            break;
                        case "hkp2000":
                            stats.P2000Assists++;
                            _logger.LogStatsUpdate(assisterName, "P2000 Assists", stats.P2000Assists);
                            break;
                        case "fiveseven":
                            stats.FiveSevenAssists++;
                            _logger.LogStatsUpdate(assisterName, "FiveSeven Assists", stats.FiveSevenAssists);
                            break;
                        case "tec9":
                            stats.Tec9Assists++;
                            _logger.LogStatsUpdate(assisterName, "Tec9 Assists", stats.Tec9Assists);
                            break;
                        case "cz75a":
                            stats.CZ75Assists++;
                            _logger.LogStatsUpdate(assisterName, "CZ75 Assists", stats.CZ75Assists);
                            break;
                        case "elite":
                            stats.DualiesAssists++;
                            _logger.LogStatsUpdate(assisterName, "Dualies Assists", stats.DualiesAssists);
                            break;
                        case "revolver":
                            stats.RevolverAssists++;
                            _logger.LogStatsUpdate(assisterName, "Revolver Assists", stats.RevolverAssists);
                            break;
                        case "nova":
                            stats.NovaAssists++;
                            _logger.LogStatsUpdate(assisterName, "Nova Assists", stats.NovaAssists);
                            break;
                        case "xm1014":
                            stats.XM1014Assists++;
                            _logger.LogStatsUpdate(assisterName, "XM1014 Assists", stats.XM1014Assists);
                            break;
                        case "sawedoff":
                            stats.SawedOffAssists++;
                            _logger.LogStatsUpdate(assisterName, "SawedOff Assists", stats.SawedOffAssists);
                            break;
                        case "mag7":
                            stats.MAG7Assists++;
                            _logger.LogStatsUpdate(assisterName, "MAG7 Assists", stats.MAG7Assists);
                            break;
                        case "mac10":
                            stats.Mac10Assists++;
                            _logger.LogStatsUpdate(assisterName, "Mac10 Assists", stats.Mac10Assists);
                            break;
                        case "mp9":
                            stats.MP9Assists++;
                            _logger.LogStatsUpdate(assisterName, "MP9 Assists", stats.MP9Assists);
                            break;
                        case "mp7":
                            stats.MP7Assists++;
                            _logger.LogStatsUpdate(assisterName, "MP7 Assists", stats.MP7Assists);
                            break;
                        case "ump45":
                            stats.UMP45Assists++;
                            _logger.LogStatsUpdate(assisterName, "UMP45 Assists", stats.UMP45Assists);
                            break;
                        case "p90":
                            stats.P90Assists++;
                            _logger.LogStatsUpdate(assisterName, "P90 Assists", stats.P90Assists);
                            break;
                        case "bizon":
                            stats.BizonAssists++;
                            _logger.LogStatsUpdate(assisterName, "Bizon Assists", stats.BizonAssists);
                            break;
                        case "negev":
                            stats.NegevAssists++;
                            _logger.LogStatsUpdate(assisterName, "Negev Assists", stats.NegevAssists);
                            break;
                        case "m249":
                            stats.M249Assists++;
                            _logger.LogStatsUpdate(assisterName, "M249 Assists", stats.M249Assists);
                            break;
                        // ... más armas según sea necesario
                    }
                    
                    // Ahora podemos usar el nombre del atacante
                    _logger.LogPlayerAssist(assisterName, attackerName, weaponName);
                }
                catch (Exception ex)
                {
                    _internalLogger?.LogError($"Error updating assist stats: {ex.Message}");
                }
            });
        }

        public void SaveAllStats()
        {
            // Enqueue the operation to run on the main thread
            MainThreadDispatcher.Enqueue(() => {
                try
                {
                    // Crear una copia de los datos de los jugadores para evitar problemas de concurrencia
                    var cachedStatsCopy = new Dictionary<string, PlayerStats>();
                    var playerNames = new Dictionary<string, string>();
                    
                    // Obtener los datos de los jugadores conectados
                    var players = Utilities.GetPlayers();
                    
                    foreach (var player in players)
                    {
                        if (player != null && player.IsValid)
                        {
                            var steamId = player.SteamID.ToString();
                            playerNames[steamId] = player.PlayerName;
                            
                            if (_cachedStats.ContainsKey(steamId))
                            {
                                // Crear una copia de las estadísticas para evitar problemas de concurrencia
                                var originalStats = _cachedStats[steamId];
                                var statsCopy = new PlayerStats
                                {
                                    Id = originalStats.Id,
                                    SteamId = originalStats.SteamId,
                                    PlayerName = player.PlayerName,
                                    TotalKills = originalStats.TotalKills,
                                    TotalDeaths = originalStats.TotalDeaths,
                                    TotalAssists = originalStats.TotalAssists,
                                    TotalHeadshots = originalStats.TotalHeadshots,
                                    TotalRoundsPlayed = originalStats.TotalRoundsPlayed,
                                    TotalDamageDone = originalStats.TotalDamageDone,
                                    TotalDamageTaken = originalStats.TotalDamageTaken,
                                    // Copiar todas las demás propiedades según sea necesario
                                    AK47Kills = originalStats.AK47Kills,
                                    M4A4Kills = originalStats.M4A4Kills,
                                    M4A1SKills = originalStats.M4A1SKills,
                                    AUGKills = originalStats.AUGKills,
                                    SG553Kills = originalStats.SG553Kills,
                                    GalilARKills = originalStats.GalilARKills,
                                    FAMASKills = originalStats.FAMASKills,
                                    AWPKills = originalStats.AWPKills,
                                    ScoutKills = originalStats.ScoutKills,
                                    Scar20Kills = originalStats.Scar20Kills,
                                    G3SG1Kills = originalStats.G3SG1Kills,
                                    SSG08Kills = originalStats.SSG08Kills,
                                    DeagleKills = originalStats.DeagleKills,
                                    GlockKills = originalStats.GlockKills,
                                    USPSKills = originalStats.USPSKills,
                                    P250Kills = originalStats.P250Kills,
                                    P2000Kills = originalStats.P2000Kills,
                                    FiveSevenKills = originalStats.FiveSevenKills,
                                    Tec9Kills = originalStats.Tec9Kills,
                                    CZ75Kills = originalStats.CZ75Kills,
                                    DualiesKills = originalStats.DualiesKills,
                                    RevolverKills = originalStats.RevolverKills,
                                    NovaKills = originalStats.NovaKills,
                                    XM1014Kills = originalStats.XM1014Kills,
                                    SawedOffKills = originalStats.SawedOffKills,
                                    MAG7Kills = originalStats.MAG7Kills,
                                    Mac10Kills = originalStats.Mac10Kills,
                                    MP9Kills = originalStats.MP9Kills,
                                    MP7Kills = originalStats.MP7Kills,
                                    UMP45Kills = originalStats.UMP45Kills,
                                    P90Kills = originalStats.P90Kills,
                                    BizonKills = originalStats.BizonKills,
                                    NegevKills = originalStats.NegevKills,
                                    M249Kills = originalStats.M249Kills,
                                    AK47Deaths = originalStats.AK47Deaths,
                                    M4A4Deaths = originalStats.M4A4Deaths,
                                    M4A1SDeaths = originalStats.M4A1SDeaths,
                                    AUGDeaths = originalStats.AUGDeaths,
                                    SG553Deaths = originalStats.SG553Deaths,
                                    GalilARDeaths = originalStats.GalilARDeaths,
                                    FAMASDeaths = originalStats.FAMASDeaths,
                                    AWPDeaths = originalStats.AWPDeaths,
                                    ScoutDeaths = originalStats.ScoutDeaths,
                                    Scar20Deaths = originalStats.Scar20Deaths,
                                    G3SG1Deaths = originalStats.G3SG1Deaths,
                                    SSG08Deaths = originalStats.SSG08Deaths,
                                    DeagleDeaths = originalStats.DeagleDeaths,
                                    GlockDeaths = originalStats.GlockDeaths,
                                    USPSDeaths = originalStats.USPSDeaths,
                                    P250Deaths = originalStats.P250Deaths,
                                    P2000Deaths = originalStats.P2000Deaths,
                                    FiveSevenDeaths = originalStats.FiveSevenDeaths,
                                    Tec9Deaths = originalStats.Tec9Deaths,
                                    CZ75Deaths = originalStats.CZ75Deaths,
                                    DualiesDeaths = originalStats.DualiesDeaths,
                                    RevolverDeaths = originalStats.RevolverDeaths,
                                    NovaDeaths = originalStats.NovaDeaths,
                                    XM1014Deaths = originalStats.XM1014Deaths,
                                    SawedOffDeaths = originalStats.SawedOffDeaths,
                                    MAG7Deaths = originalStats.MAG7Deaths,
                                    Mac10Deaths = originalStats.Mac10Deaths,
                                    MP9Deaths = originalStats.MP9Deaths,
                                    MP7Deaths = originalStats.MP7Deaths,
                                    UMP45Deaths = originalStats.UMP45Deaths,
                                    P90Deaths = originalStats.P90Deaths,
                                    BizonDeaths = originalStats.BizonDeaths,
                                    NegevDeaths = originalStats.NegevDeaths,
                                    M249Deaths = originalStats.M249Deaths,
                                    AK47Assists = originalStats.AK47Assists,
                                    M4A4Assists = originalStats.M4A4Assists,
                                    M4A1SAssists = originalStats.M4A1SAssists,
                                    AUGAssists = originalStats.AUGAssists,
                                    SG553Assists = originalStats.SG553Assists,
                                    GalilARAssists = originalStats.GalilARAssists,
                                    FAMASAssists = originalStats.FAMASAssists,
                                    AWPAssists = originalStats.AWPAssists,
                                    ScoutAssists = originalStats.ScoutAssists,
                                    Scar20Assists = originalStats.Scar20Assists,
                                    G3SG1Assists = originalStats.G3SG1Assists,
                                    SSG08Assists = originalStats.SSG08Assists,
                                    DeagleAssists = originalStats.DeagleAssists,
                                    GlockAssists = originalStats.GlockAssists,
                                    USPSAssists = originalStats.USPSAssists,
                                    P250Assists = originalStats.P250Assists,
                                    P2000Assists = originalStats.P2000Assists,
                                    FiveSevenAssists = originalStats.FiveSevenAssists,
                                    Tec9Assists = originalStats.Tec9Assists,
                                    CZ75Assists = originalStats.CZ75Assists,
                                    DualiesAssists = originalStats.DualiesAssists,
                                    RevolverAssists = originalStats.RevolverAssists,
                                    NovaAssists = originalStats.NovaAssists,
                                    XM1014Assists = originalStats.XM1014Assists,
                                    SawedOffAssists = originalStats.SawedOffAssists,
                                    MAG7Assists = originalStats.MAG7Assists,
                                    Mac10Assists = originalStats.Mac10Assists,
                                    MP9Assists = originalStats.MP9Assists,
                                    MP7Assists = originalStats.MP7Assists,
                                    UMP45Assists = originalStats.UMP45Assists,
                                    P90Assists = originalStats.P90Assists,
                                    BizonAssists = originalStats.BizonAssists,
                                    NegevAssists = originalStats.NegevAssists,
                                    M249Assists = originalStats.M249Assists,
                                    AK47Headshots = originalStats.AK47Headshots,
                                    M4A4Headshots = originalStats.M4A4Headshots,
                                    M4A1SHeadshots = originalStats.M4A1SHeadshots,
                                    AUGHeadshots = originalStats.AUGHeadshots,
                                    SG553Headshots = originalStats.SG553Headshots,
                                    GalilARHeadshots = originalStats.GalilARHeadshots,
                                    FAMASHeadshots = originalStats.FAMASHeadshots,
                                    AWPHeadshots = originalStats.AWPHeadshots,
                                    ScoutHeadshots = originalStats.ScoutHeadshots,
                                    Scar20Headshots = originalStats.Scar20Headshots,
                                    G3SG1Headshots = originalStats.G3SG1Headshots,
                                    SSG08Headshots = originalStats.SSG08Headshots,
                                    DeagleHeadshots = originalStats.DeagleHeadshots,
                                    GlockHeadshots = originalStats.GlockHeadshots,
                                    USPSHeadshots = originalStats.USPSHeadshots,
                                    P250Headshots = originalStats.P250Headshots,
                                    P2000Headshots = originalStats.P2000Headshots,
                                    FiveSevenHeadshots = originalStats.FiveSevenHeadshots,
                                    Tec9Headshots = originalStats.Tec9Headshots,
                                    CZ75Headshots = originalStats.CZ75Headshots,
                                    DualiesHeadshots = originalStats.DualiesHeadshots,
                                    RevolverHeadshots = originalStats.RevolverHeadshots,
                                    NovaHeadshots = originalStats.NovaHeadshots,
                                    XM1014Headshots = originalStats.XM1014Headshots,
                                    SawedOffHeadshots = originalStats.SawedOffHeadshots,
                                    MAG7Headshots = originalStats.MAG7Headshots,
                                    Mac10Headshots = originalStats.Mac10Headshots,
                                    MP9Headshots = originalStats.MP9Headshots,
                                    MP7Headshots = originalStats.MP7Headshots,
                                    UMP45Headshots = originalStats.UMP45Headshots,
                                    P90Headshots = originalStats.P90Headshots,
                                    BizonHeadshots = originalStats.BizonHeadshots,
                                    NegevHeadshots = originalStats.NegevHeadshots,
                                    M249Headshots = originalStats.M249Headshots,
                                    AK47Chestshots = originalStats.AK47Chestshots,
                                    M4A4Chestshots = originalStats.M4A4Chestshots,
                                    M4A1SChestshots = originalStats.M4A1SChestshots,
                                    AUGChestshots = originalStats.AUGChestshots,
                                    SG553Chestshots = originalStats.SG553Chestshots,
                                    GalilARChestshots = originalStats.GalilARChestshots,
                                    FAMASChestshots = originalStats.FAMASChestshots,
                                    AWPChestshots = originalStats.AWPChestshots,
                                    ScoutChestshots = originalStats.ScoutChestshots,
                                    Scar20Chestshots = originalStats.Scar20Chestshots,
                                    G3SG1Chestshots = originalStats.G3SG1Chestshots,
                                    SSG08Chestshots = originalStats.SSG08Chestshots,
                                    DeagleChestshots = originalStats.DeagleChestshots,
                                    GlockChestshots = originalStats.GlockChestshots,
                                    USPSChestshots = originalStats.USPSChestshots,
                                    P250Chestshots = originalStats.P250Chestshots,
                                    P2000Chestshots = originalStats.P2000Chestshots,
                                    FiveSevenChestshots = originalStats.FiveSevenChestshots,
                                    Tec9Chestshots = originalStats.Tec9Chestshots,
                                    CZ75Chestshots = originalStats.CZ75Chestshots,
                                    DualiesChestshots = originalStats.DualiesChestshots,
                                    RevolverChestshots = originalStats.RevolverChestshots,
                                    NovaChestshots = originalStats.NovaChestshots,
                                    XM1014Chestshots = originalStats.XM1014Chestshots,
                                    SawedOffChestshots = originalStats.SawedOffChestshots,
                                    MAG7Chestshots = originalStats.MAG7Chestshots,
                                    Mac10Chestshots = originalStats.Mac10Chestshots,
                                    MP9Chestshots = originalStats.MP9Chestshots,
                                    MP7Chestshots = originalStats.MP7Chestshots,
                                    UMP45Chestshots = originalStats.UMP45Chestshots,
                                    P90Chestshots = originalStats.P90Chestshots,
                                    BizonChestshots = originalStats.BizonChestshots,
                                    NegevChestshots = originalStats.NegevChestshots,
                                    M249Chestshots = originalStats.M249Chestshots,
                                    AK47Stomachshots = originalStats.AK47Stomachshots,
                                    M4A4Stomachshots = originalStats.M4A4Stomachshots,
                                    M4A1SStomachshots = originalStats.M4A1SStomachshots,
                                    AUGStomachshots = originalStats.AUGStomachshots,
                                    SG553Stomachshots = originalStats.SG553Stomachshots,
                                    GalilARStomachshots = originalStats.GalilARStomachshots,
                                    FAMASStomachshots = originalStats.FAMASStomachshots,
                                    AWPStomachshots = originalStats.AWPStomachshots,
                                    ScoutStomachshots = originalStats.ScoutStomachshots,
                                    Scar20Stomachshots = originalStats.Scar20Stomachshots,
                                    G3SG1Stomachshots = originalStats.G3SG1Stomachshots,
                                    SSG08Stomachshots = originalStats.SSG08Stomachshots,
                                    DeagleStomachshots = originalStats.DeagleStomachshots,
                                    GlockStomachshots = originalStats.GlockStomachshots,
                                    USPSStomachshots = originalStats.USPSStomachshots,
                                    P250Stomachshots = originalStats.P250Stomachshots,
                                    P2000Stomachshots = originalStats.P2000Stomachshots,
                                    FiveSevenStomachshots = originalStats.FiveSevenStomachshots,
                                    Tec9Stomachshots = originalStats.Tec9Stomachshots,
                                    CZ75Stomachshots = originalStats.CZ75Stomachshots,
                                    DualiesStomachshots = originalStats.DualiesStomachshots,
                                    RevolverStomachshots = originalStats.RevolverStomachshots,
                                    NovaStomachshots = originalStats.NovaStomachshots,
                                    XM1014Stomachshots = originalStats.XM1014Stomachshots,
                                    SawedOffStomachshots = originalStats.SawedOffStomachshots,
                                    MAG7Stomachshots = originalStats.MAG7Stomachshots,
                                    Mac10Stomachshots = originalStats.Mac10Stomachshots,
                                    MP9Stomachshots = originalStats.MP9Stomachshots,
                                    MP7Stomachshots = originalStats.MP7Stomachshots,
                                    UMP45Stomachshots = originalStats.UMP45Stomachshots,
                                    P90Stomachshots = originalStats.P90Stomachshots,
                                    BizonStomachshots = originalStats.BizonStomachshots,
                                    NegevStomachshots = originalStats.NegevStomachshots,
                                    M249Stomachshots = originalStats.M249Stomachshots,
                                    AK47LeftLegshots = originalStats.AK47LeftLegshots,
                                    M4A4LeftLegshots = originalStats.M4A4LeftLegshots,
                                    M4A1SLeftLegshots = originalStats.M4A1SLeftLegshots,
                                    AUGLeftLegshots = originalStats.AUGLeftLegshots,
                                    SG553LeftLegshots = originalStats.SG553LeftLegshots,
                                    GalilARLeftLegshots = originalStats.GalilARLeftLegshots,
                                    FAMASLeftLegshots = originalStats.FAMASLeftLegshots,
                                    AWPLeftLegshots = originalStats.AWPLeftLegshots,
                                    ScoutLeftLegshots = originalStats.ScoutLeftLegshots,
                                    Scar20LeftLegshots = originalStats.Scar20LeftLegshots,
                                    G3SG1LeftLegshots = originalStats.G3SG1LeftLegshots,
                                    SSG08LeftLegshots = originalStats.SSG08LeftLegshots,
                                    DeagleLeftLegshots = originalStats.DeagleLeftLegshots,
                                    GlockLeftLegshots = originalStats.GlockLeftLegshots,
                                    USPSLeftLegshots = originalStats.USPSLeftLegshots,
                                    P250LeftLegshots = originalStats.P250LeftLegshots,
                                    P2000LeftLegshots = originalStats.P2000LeftLegshots,
                                    FiveSevenLeftLegshots = originalStats.FiveSevenLeftLegshots,
                                    Tec9LeftLegshots = originalStats.Tec9LeftLegshots,
                                    CZ75LeftLegshots = originalStats.CZ75LeftLegshots,
                                    DualiesLeftLegshots = originalStats.DualiesLeftLegshots,
                                    RevolverLeftLegshots = originalStats.RevolverLeftLegshots,
                                    NovaLeftLegshots = originalStats.NovaLeftLegshots,
                                    XM1014LeftLegshots = originalStats.XM1014LeftLegshots,
                                    SawedOffLeftLegshots = originalStats.SawedOffLeftLegshots,
                                    MAG7LeftLegshots = originalStats.MAG7LeftLegshots,
                                    Mac10LeftLegshots = originalStats.Mac10LeftLegshots,
                                    MP9LeftLegshots = originalStats.MP9LeftLegshots,
                                    MP7LeftLegshots = originalStats.MP7LeftLegshots,
                                    UMP45LeftLegshots = originalStats.UMP45LeftLegshots,
                                    P90LeftLegshots = originalStats.P90LeftLegshots,
                                    BizonLeftLegshots = originalStats.BizonLeftLegshots,
                                    NegevLeftLegshots = originalStats.NegevLeftLegshots,
                                    M249LeftLegshots = originalStats.M249LeftLegshots,
                                    AK47RightLegshots = originalStats.AK47RightLegshots,
                                    M4A4RightLegshots = originalStats.M4A4RightLegshots,
                                    M4A1SRightLegshots = originalStats.M4A1SRightLegshots,
                                    AUGRightLegshots = originalStats.AUGRightLegshots,
                                    SG553RightLegshots = originalStats.SG553RightLegshots,
                                    GalilARRightLegshots = originalStats.GalilARRightLegshots,
                                    FAMASRightLegshots = originalStats.FAMASRightLegshots,
                                    AWPRightLegshots = originalStats.AWPRightLegshots,
                                    ScoutRightLegshots = originalStats.ScoutRightLegshots,
                                    Scar20RightLegshots = originalStats.Scar20RightLegshots,
                                    G3SG1RightLegshots = originalStats.G3SG1RightLegshots,
                                    SSG08RightLegshots = originalStats.SSG08RightLegshots,
                                    DeagleRightLegshots = originalStats.DeagleRightLegshots,
                                    GlockRightLegshots = originalStats.GlockRightLegshots,
                                    USPSRightLegshots = originalStats.USPSRightLegshots,
                                    P250RightLegshots = originalStats.P250RightLegshots,
                                    P2000RightLegshots = originalStats.P2000RightLegshots,
                                    FiveSevenRightLegshots = originalStats.FiveSevenRightLegshots,
                                    Tec9RightLegshots = originalStats.Tec9RightLegshots,
                                    CZ75RightLegshots = originalStats.CZ75RightLegshots,
                                    DualiesRightLegshots = originalStats.DualiesRightLegshots,
                                    RevolverRightLegshots = originalStats.RevolverRightLegshots,
                                    NovaRightLegshots = originalStats.NovaRightLegshots,
                                    XM1014RightLegshots = originalStats.XM1014RightLegshots,
                                    SawedOffRightLegshots = originalStats.SawedOffRightLegshots,
                                    MAG7RightLegshots = originalStats.MAG7RightLegshots,
                                    Mac10RightLegshots = originalStats.Mac10RightLegshots,
                                    MP9RightLegshots = originalStats.MP9RightLegshots,
                                    MP7RightLegshots = originalStats.MP7RightLegshots,
                                    UMP45RightLegshots = originalStats.UMP45RightLegshots,
                                    P90RightLegshots = originalStats.P90RightLegshots,
                                    BizonRightLegshots = originalStats.BizonRightLegshots,
                                    NegevRightLegshots = originalStats.NegevRightLegshots,
                                    M249RightLegshots = originalStats.M249RightLegshots,
                                    LastUpdated = DateTime.UtcNow
                                };
                                
                                cachedStatsCopy[steamId] = statsCopy;
                            }
                        }
                    }
                    
                    // Guardar las estadísticas usando la copia de datos
                    foreach (var kvp in cachedStatsCopy)
                    {
                        var steamId = kvp.Key;
                        var stats = kvp.Value;
                        
                        if (playerNames.ContainsKey(steamId))
                        {
                            stats.PlayerName = playerNames[steamId];
                            _databaseManager.UpdatePlayerStatsAsync(stats).Wait();
                            _logger.LogInfo($"Saved stats for player {playerNames[steamId]} ({steamId})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _internalLogger?.LogError($"Error saving all stats: {ex.Message}");
                }
            });
        }
    }
}