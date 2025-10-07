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
        private ILogger? _logger;
        private Dictionary<string, PlayerStats> _cachedStats = new();

        public StatsService(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
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
            }
            
            _cachedStats[steamId] = stats;
            return stats;
        }

        public async Task SavePlayerStatsAsync(CCSPlayerController player)
        {
            var steamId = player.SteamID.ToString();
            
            if (_cachedStats.ContainsKey(steamId))
            {
                var stats = _cachedStats[steamId];
                stats.PlayerName = player.PlayerName;
                stats.LastUpdated = DateTime.UtcNow;
                
                await _databaseManager.UpdatePlayerStatsAsync(stats);
            }
        }

        public void TrackKill(CCSPlayerController attacker, CCSPlayerController victim, string weaponName, bool headshot)
        {
            // Verificar si debemos rastrear eventos de bots
            if ((attacker.IsBot || victim.IsBot))
            {
                // Aquí iría la lógica de configuración para determinar si se rastrean bots
                // Por ahora dejamos la implementación básica
                return;
            }
            
            // Actualizar estadísticas del atacante
            var attackerTask = UpdateAttackerStats(attacker, weaponName, headshot);
            
            // Actualizar estadísticas de la víctima
            var victimTask = UpdateVictimStats(victim, weaponName);
            
            // No esperamos las tareas porque son operaciones asíncronas en segundo plano
        }

        public void TrackKillWithHitGroup(CCSPlayerController attacker, CCSPlayerController victim, string weaponName, int hitGroup)
        {
            // Verificar si debemos rastrear eventos de bots
            if ((attacker.IsBot || victim.IsBot))
            {
                // Aquí iría la lógica de configuración para determinar si se rastrean bots
                return;
            }
            
            // Actualizar estadísticas del atacante con hitgroup
            var attackerTask = UpdateAttackerStatsWithHitGroup(attacker, weaponName, hitGroup);
            
            // Actualizar estadísticas de la víctima
            var victimTask = UpdateVictimStats(victim, weaponName);
        }

        public void TrackAssist(CCSPlayerController assister, string weaponName)
        {
            if (assister.IsBot)
            {
                // Verificar configuración para rastrear bots
                return;
            }
            
            var assistTask = UpdateAssistStats(assister, weaponName);
        }

        private async Task UpdateAttackerStats(CCSPlayerController attacker, string weaponName, bool headshot)
        {
            try
            {
                var stats = await GetOrCreatePlayerStatsAsync(attacker);
                
                stats.TotalKills++;
                
                if (headshot)
                {
                    stats.TotalHeadshots++;
                }
                
                // Incrementar kills específicos por arma
                switch (weaponName.ToLower())
                {
                    case "ak47":
                        stats.AK47Kills++;
                        if (headshot) stats.AK47Headshots++;
                        break;
                    case "m4a4":
                        stats.M4A4Kills++;
                        if (headshot) stats.M4A4Headshots++;
                        break;
                    case "m4a1_silencer":
                    case "m4a1_silencer_off":
                        stats.M4A1SKills++;
                        if (headshot) stats.M4A1SHeadshots++;
                        break;
                    case "aug":
                        stats.AUGKills++;
                        if (headshot) stats.AUGHeadshots++;
                        break;
                    case "sg556":
                        stats.SG553Kills++;
                        if (headshot) stats.SG553Headshots++;
                        break;
                    case "galilar":
                        stats.GalilARKills++;
                        if (headshot) stats.GalilARHeadshots++;
                        break;
                    case "famas":
                        stats.FAMASKills++;
                        if (headshot) stats.FAMASHeadshots++;
                        break;
                    case "awp":
                        stats.AWPKills++;
                        if (headshot) stats.AWPHeadshots++;
                        break;
                    case "ssg08":
                        stats.SSG08Kills++;
                        if (headshot) stats.SSG08Headshots++;
                        break;
                    case "scar20":
                        stats.Scar20Kills++;
                        if (headshot) stats.Scar20Headshots++;
                        break;
                    case "g3sg1":
                        stats.G3SG1Kills++;
                        if (headshot) stats.G3SG1Headshots++;
                        break;
                    case "deagle":
                        stats.DeagleKills++;
                        if (headshot) stats.DeagleHeadshots++;
                        break;
                    case "glock":
                        stats.GlockKills++;
                        if (headshot) stats.GlockHeadshots++;
                        break;
                    case "usp_silencer":
                    case "usp_silencer_off":
                        stats.USPSKills++;
                        if (headshot) stats.USPSHeadshots++;
                        break;
                    case "p250":
                        stats.P250Kills++;
                        if (headshot) stats.P250Headshots++;
                        break;
                    case "hkp2000":
                        stats.P2000Kills++;
                        if (headshot) stats.P2000Headshots++;
                        break;
                    case "fiveseven":
                        stats.FiveSevenKills++;
                        if (headshot) stats.FiveSevenHeadshots++;
                        break;
                    case "tec9":
                        stats.Tec9Kills++;
                        if (headshot) stats.Tec9Headshots++;
                        break;
                    case "cz75a":
                        stats.CZ75Kills++;
                        if (headshot) stats.CZ75Headshots++;
                        break;
                    case "elite":
                        stats.DualiesKills++;
                        if (headshot) stats.DualiesHeadshots++;
                        break;
                    case "revolver":
                        stats.RevolverKills++;
                        if (headshot) stats.RevolverHeadshots++;
                        break;
                    // ... más armas según sea necesario
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error updating attacker stats: {ex.Message}");
            }
        }

        private async Task UpdateAttackerStatsWithHitGroup(CCSPlayerController attacker, string weaponName, int hitGroup)
        {
            try
            {
                var stats = await GetOrCreatePlayerStatsAsync(attacker);
                
                stats.TotalKills++;
                
                // Determinar el tipo de hit basado en el hitGroup
                switch (hitGroup)
                {
                    case 1: // Head
                        stats.TotalHeadshots++;
                        break;
                }
                
                // Convertir hitGroup a enum para mejor legibilidad
                var hitGroupEnum = (HitGroup)hitGroup;
                
                // Incrementar estadísticas específicas por arma y hitgroup
                switch (weaponName.ToLower())
                {
                    case "ak47":
                        stats.AK47Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.AK47Headshots++; break;
                            case HitGroup.Chest: stats.AK47Chestshots++; break;
                            case HitGroup.Stomach: stats.AK47Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.AK47LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.AK47RightLegshots++; break;
                        }
                        break;
                    case "m4a4":
                        stats.M4A4Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.M4A4Headshots++; break;
                            case HitGroup.Chest: stats.M4A4Chestshots++; break;
                            case HitGroup.Stomach: stats.M4A4Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.M4A4LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.M4A4RightLegshots++; break;
                        }
                        break;
                    case "m4a1_silencer":
                    case "m4a1_silencer_off":
                        stats.M4A1SKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.M4A1SHeadshots++; break;
                            case HitGroup.Chest: stats.M4A1SChestshots++; break;
                            case HitGroup.Stomach: stats.M4A1SStomachshots++; break;
                            case HitGroup.LeftLeg: stats.M4A1SLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.M4A1SRightLegshots++; break;
                        }
                        break;
                    case "aug":
                        stats.AUGKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.AUGHeadshots++; break;
                            case HitGroup.Chest: stats.AUGChestshots++; break;
                            case HitGroup.Stomach: stats.AUGStomachshots++; break;
                            case HitGroup.LeftLeg: stats.AUGLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.AUGRightLegshots++; break;
                        }
                        break;
                    case "sg556":
                        stats.SG553Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.SG553Headshots++; break;
                            case HitGroup.Chest: stats.SG553Chestshots++; break;
                            case HitGroup.Stomach: stats.SG553Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.SG553LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.SG553RightLegshots++; break;
                        }
                        break;
                    case "galilar":
                        stats.GalilARKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.GalilARHeadshots++; break;
                            case HitGroup.Chest: stats.GalilARChestshots++; break;
                            case HitGroup.Stomach: stats.GalilARStomachshots++; break;
                            case HitGroup.LeftLeg: stats.GalilARLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.GalilARRightLegshots++; break;
                        }
                        break;
                    case "famas":
                        stats.FAMASKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.FAMASHeadshots++; break;
                            case HitGroup.Chest: stats.FAMASChestshots++; break;
                            case HitGroup.Stomach: stats.FAMASStomachshots++; break;
                            case HitGroup.LeftLeg: stats.FAMASLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.FAMASRightLegshots++; break;
                        }
                        break;
                    case "awp":
                        stats.AWPKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.AWPHeadshots++; break;
                            case HitGroup.Chest: stats.AWPChestshots++; break;
                            case HitGroup.Stomach: stats.AWPStomachshots++; break;
                            case HitGroup.LeftLeg: stats.AWPLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.AWPRightLegshots++; break;
                        }
                        break;
                    case "ssg08":
                        stats.SSG08Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.SSG08Headshots++; break;
                            case HitGroup.Chest: stats.SSG08Chestshots++; break;
                            case HitGroup.Stomach: stats.SSG08Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.SSG08LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.SSG08RightLegshots++; break;
                        }
                        break;
                    case "scar20":
                        stats.Scar20Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.Scar20Headshots++; break;
                            case HitGroup.Chest: stats.Scar20Chestshots++; break;
                            case HitGroup.Stomach: stats.Scar20Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.Scar20LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.Scar20RightLegshots++; break;
                        }
                        break;
                    case "g3sg1":
                        stats.G3SG1Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.G3SG1Headshots++; break;
                            case HitGroup.Chest: stats.G3SG1Chestshots++; break;
                            case HitGroup.Stomach: stats.G3SG1Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.G3SG1LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.G3SG1RightLegshots++; break;
                        }
                        break;
                    case "deagle":
                        stats.DeagleKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.DeagleHeadshots++; break;
                            case HitGroup.Chest: stats.DeagleChestshots++; break;
                            case HitGroup.Stomach: stats.DeagleStomachshots++; break;
                            case HitGroup.LeftLeg: stats.DeagleLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.DeagleRightLegshots++; break;
                        }
                        break;
                    case "glock":
                        stats.GlockKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.GlockHeadshots++; break;
                            case HitGroup.Chest: stats.GlockChestshots++; break;
                            case HitGroup.Stomach: stats.GlockStomachshots++; break;
                            case HitGroup.LeftLeg: stats.GlockLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.GlockRightLegshots++; break;
                        }
                        break;
                    case "usp_silencer":
                    case "usp_silencer_off":
                        stats.USPSKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.USPSHeadshots++; break;
                            case HitGroup.Chest: stats.USPSChestshots++; break;
                            case HitGroup.Stomach: stats.USPSStomachshots++; break;
                            case HitGroup.LeftLeg: stats.USPSLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.USPSRightLegshots++; break;
                        }
                        break;
                    case "p250":
                        stats.P250Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.P250Headshots++; break;
                            case HitGroup.Chest: stats.P250Chestshots++; break;
                            case HitGroup.Stomach: stats.P250Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.P250LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.P250RightLegshots++; break;
                        }
                        break;
                    case "hkp2000":
                        stats.P2000Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.P2000Headshots++; break;
                            case HitGroup.Chest: stats.P2000Chestshots++; break;
                            case HitGroup.Stomach: stats.P2000Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.P2000LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.P2000RightLegshots++; break;
                        }
                        break;
                    case "fiveseven":
                        stats.FiveSevenKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.FiveSevenHeadshots++; break;
                            case HitGroup.Chest: stats.FiveSevenChestshots++; break;
                            case HitGroup.Stomach: stats.FiveSevenStomachshots++; break;
                            case HitGroup.LeftLeg: stats.FiveSevenLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.FiveSevenRightLegshots++; break;
                        }
                        break;
                    case "tec9":
                        stats.Tec9Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.Tec9Headshots++; break;
                            case HitGroup.Chest: stats.Tec9Chestshots++; break;
                            case HitGroup.Stomach: stats.Tec9Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.Tec9LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.Tec9RightLegshots++; break;
                        }
                        break;
                    case "cz75a":
                        stats.CZ75Kills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.CZ75Headshots++; break;
                            case HitGroup.Chest: stats.CZ75Chestshots++; break;
                            case HitGroup.Stomach: stats.CZ75Stomachshots++; break;
                            case HitGroup.LeftLeg: stats.CZ75LeftLegshots++; break;
                            case HitGroup.RightLeg: stats.CZ75RightLegshots++; break;
                        }
                        break;
                    case "elite":
                        stats.DualiesKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.DualiesHeadshots++; break;
                            case HitGroup.Chest: stats.DualiesChestshots++; break;
                            case HitGroup.Stomach: stats.DualiesStomachshots++; break;
                            case HitGroup.LeftLeg: stats.DualiesLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.DualiesRightLegshots++; break;
                        }
                        break;
                    case "revolver":
                        stats.RevolverKills++;
                        switch (hitGroupEnum)
                        {
                            case HitGroup.Head: stats.RevolverHeadshots++; break;
                            case HitGroup.Chest: stats.RevolverChestshots++; break;
                            case HitGroup.Stomach: stats.RevolverStomachshots++; break;
                            case HitGroup.LeftLeg: stats.RevolverLeftLegshots++; break;
                            case HitGroup.RightLeg: stats.RevolverRightLegshots++; break;
                        }
                        break;
                    // ... más armas según sea necesario
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error updating attacker stats with hitgroup: {ex.Message}");
            }
        }

        private async Task UpdateVictimStats(CCSPlayerController victim, string weaponName)
        {
            try
            {
                var stats = await GetOrCreatePlayerStatsAsync(victim);
                stats.TotalDeaths++;
                
                // Incrementar muertes específicas por arma
                switch (weaponName.ToLower())
                {
                    case "ak47":
                        stats.AK47Deaths++;
                        break;
                    case "m4a4":
                        stats.M4A4Deaths++;
                        break;
                    case "m4a1_silencer":
                    case "m4a1_silencer_off":
                        stats.M4A1SDeaths++;
                        break;
                    case "aug":
                        stats.AUGDeaths++;
                        break;
                    case "sg556":
                        stats.SG553Deaths++;
                        break;
                    case "galilar":
                        stats.GalilARDeaths++;
                        break;
                    case "famas":
                        stats.FAMASDeaths++;
                        break;
                    case "awp":
                        stats.AWPDeaths++;
                        break;
                    case "ssg08":
                        stats.SSG08Deaths++;
                        break;
                    case "scar20":
                        stats.Scar20Deaths++;
                        break;
                    case "g3sg1":
                        stats.G3SG1Deaths++;
                        break;
                    case "deagle":
                        stats.DeagleDeaths++;
                        break;
                    case "glock":
                        stats.GlockDeaths++;
                        break;
                    case "usp_silencer":
                    case "usp_silencer_off":
                        stats.USPSDeaths++;
                        break;
                    case "p250":
                        stats.P250Deaths++;
                        break;
                    case "hkp2000":
                        stats.P2000Deaths++;
                        break;
                    case "fiveseven":
                        stats.FiveSevenDeaths++;
                        break;
                    case "tec9":
                        stats.Tec9Deaths++;
                        break;
                    case "cz75a":
                        stats.CZ75Deaths++;
                        break;
                    case "elite":
                        stats.DualiesDeaths++;
                        break;
                    case "revolver":
                        stats.RevolverDeaths++;
                        break;
                    // ... más armas según sea necesario
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error updating victim stats: {ex.Message}");
            }
        }

        private async Task UpdateAssistStats(CCSPlayerController assister, string weaponName)
        {
            try
            {
                var stats = await GetOrCreatePlayerStatsAsync(assister);
                stats.TotalAssists++;
                
                // Incrementar asistencias específicas por arma
                switch (weaponName.ToLower())
                {
                    case "ak47":
                        stats.AK47Assists++;
                        break;
                    case "m4a4":
                        stats.M4A4Assists++;
                        break;
                    case "m4a1_silencer":
                    case "m4a1_silencer_off":
                        stats.M4A1SAssists++;
                        break;
                    case "aug":
                        stats.AUGAssists++;
                        break;
                    case "sg556":
                        stats.SG553Assists++;
                        break;
                    case "galilar":
                        stats.GalilARAssists++;
                        break;
                    case "famas":
                        stats.FAMASAssists++;
                        break;
                    case "awp":
                        stats.AWPAssists++;
                        break;
                    case "ssg08":
                        stats.SSG08Assists++;
                        break;
                    case "scar20":
                        stats.Scar20Assists++;
                        break;
                    case "g3sg1":
                        stats.G3SG1Assists++;
                        break;
                    case "deagle":
                        stats.DeagleAssists++;
                        break;
                    case "glock":
                        stats.GlockAssists++;
                        break;
                    case "usp_silencer":
                    case "usp_silencer_off":
                        stats.USPSAssists++;
                        break;
                    case "p250":
                        stats.P250Assists++;
                        break;
                    case "hkp2000":
                        stats.P2000Assists++;
                        break;
                    case "fiveseven":
                        stats.FiveSevenAssists++;
                        break;
                    case "tec9":
                        stats.Tec9Assists++;
                        break;
                    case "cz75a":
                        stats.CZ75Assists++;
                        break;
                    case "elite":
                        stats.DualiesAssists++;
                        break;
                    case "revolver":
                        stats.RevolverAssists++;
                        break;
                    // ... más armas según sea necesario
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error updating assist stats: {ex.Message}");
            }
        }

        public async Task SaveAllStatsAsync()
        {
            foreach (var kvp in _cachedStats)
            {
                // Encontrar el jugador por SteamID
                var player = Utilities.GetPlayers().FirstOrDefault(p => p.SteamID.ToString() == kvp.Key);
                if (player != null)
                {
                    await _databaseManager.UpdatePlayerStatsAsync(kvp.Value);
                }
            }
        }
    }
}