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
        private Dictionary<string, PlayerStats> _cachedStats = new();

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
                                    LastUpdated = DateTime.UtcNow
                                    // Copiar todas las demás propiedades según sea necesario
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