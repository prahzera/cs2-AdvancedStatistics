using System.Data;
using MySql.Data.MySqlClient;
using AdvancedStatistics.Config;
using AdvancedStatistics.Models;
using Microsoft.Extensions.Logging;

namespace AdvancedStatistics.Database
{
    public class DatabaseManager
    {
        private readonly PluginConfig _config;
        private readonly string _connectionString;
        private ILogger? _logger;

        public DatabaseManager(PluginConfig config)
        {
            _config = config;
            _connectionString = $"Server={_config.DatabaseHost};Port={_config.DatabasePort};" +
                               $"Database={_config.DatabaseName};Uid={_config.DatabaseUser};" +
                               $"Pwd={_config.DatabasePassword};";
        }

        public void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Database connection failed: {ex.Message}");
                return false;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS player_stats (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        steam_id VARCHAR(64) UNIQUE NOT NULL,
                        player_name VARCHAR(128) NOT NULL,
                        total_kills INT DEFAULT 0,
                        total_deaths INT DEFAULT 0,
                        total_assists INT DEFAULT 0,
                        total_headshots INT DEFAULT 0,
                        total_rounds_played INT DEFAULT 0,
                        total_damage_done INT DEFAULT 0,
                        total_damage_taken INT DEFAULT 0,
                        ak47_kills INT DEFAULT 0,
                        m4a4_kills INT DEFAULT 0,
                        m4a1s_kills INT DEFAULT 0,
                        aug_kills INT DEFAULT 0,
                        sg553_kills INT DEFAULT 0,
                        galilar_kills INT DEFAULT 0,
                        famas_kills INT DEFAULT 0,
                        awp_kills INT DEFAULT 0,
                        scout_kills INT DEFAULT 0,
                        scar20_kills INT DEFAULT 0,
                        g3sg1_kills INT DEFAULT 0,
                        ssg08_kills INT DEFAULT 0,
                        deagle_kills INT DEFAULT 0,
                        glock_kills INT DEFAULT 0,
                        usps_kills INT DEFAULT 0,
                        p250_kills INT DEFAULT 0,
                        p2000_kills INT DEFAULT 0,
                        fiveseven_kills INT DEFAULT 0,
                        tec9_kills INT DEFAULT 0,
                        cz75_kills INT DEFAULT 0,
                        dualies_kills INT DEFAULT 0,
                        revolver_kills INT DEFAULT 0,
                        nova_kills INT DEFAULT 0,
                        xm1014_kills INT DEFAULT 0,
                        sawedoff_kills INT DEFAULT 0,
                        mag7_kills INT DEFAULT 0,
                        mac10_kills INT DEFAULT 0,
                        mp9_kills INT DEFAULT 0,
                        mp7_kills INT DEFAULT 0,
                        ump45_kills INT DEFAULT 0,
                        p90_kills INT DEFAULT 0,
                        bizon_kills INT DEFAULT 0,
                        negev_kills INT DEFAULT 0,
                        m249_kills INT DEFAULT 0,
                        ak47_deaths INT DEFAULT 0,
                        m4a4_deaths INT DEFAULT 0,
                        m4a1s_deaths INT DEFAULT 0,
                        aug_deaths INT DEFAULT 0,
                        sg553_deaths INT DEFAULT 0,
                        galilar_deaths INT DEFAULT 0,
                        famas_deaths INT DEFAULT 0,
                        awp_deaths INT DEFAULT 0,
                        scout_deaths INT DEFAULT 0,
                        scar20_deaths INT DEFAULT 0,
                        g3sg1_deaths INT DEFAULT 0,
                        ssg08_deaths INT DEFAULT 0,
                        deagle_deaths INT DEFAULT 0,
                        glock_deaths INT DEFAULT 0,
                        usps_deaths INT DEFAULT 0,
                        p250_deaths INT DEFAULT 0,
                        p2000_deaths INT DEFAULT 0,
                        fiveseven_deaths INT DEFAULT 0,
                        tec9_deaths INT DEFAULT 0,
                        cz75_deaths INT DEFAULT 0,
                        dualies_deaths INT DEFAULT 0,
                        revolver_deaths INT DEFAULT 0,
                        nova_deaths INT DEFAULT 0,
                        xm1014_deaths INT DEFAULT 0,
                        sawedoff_deaths INT DEFAULT 0,
                        mag7_deaths INT DEFAULT 0,
                        mac10_deaths INT DEFAULT 0,
                        mp9_deaths INT DEFAULT 0,
                        mp7_deaths INT DEFAULT 0,
                        ump45_deaths INT DEFAULT 0,
                        p90_deaths INT DEFAULT 0,
                        bizon_deaths INT DEFAULT 0,
                        negev_deaths INT DEFAULT 0,
                        m249_deaths INT DEFAULT 0,
                        ak47_assists INT DEFAULT 0,
                        m4a4_assists INT DEFAULT 0,
                        m4a1s_assists INT DEFAULT 0,
                        aug_assists INT DEFAULT 0,
                        sg553_assists INT DEFAULT 0,
                        galilar_assists INT DEFAULT 0,
                        famas_assists INT DEFAULT 0,
                        awp_assists INT DEFAULT 0,
                        scout_assists INT DEFAULT 0,
                        scar20_assists INT DEFAULT 0,
                        g3sg1_assists INT DEFAULT 0,
                        ssg08_assists INT DEFAULT 0,
                        deagle_assists INT DEFAULT 0,
                        glock_assists INT DEFAULT 0,
                        usps_assists INT DEFAULT 0,
                        p250_assists INT DEFAULT 0,
                        p2000_assists INT DEFAULT 0,
                        fiveseven_assists INT DEFAULT 0,
                        tec9_assists INT DEFAULT 0,
                        cz75_assists INT DEFAULT 0,
                        dualies_assists INT DEFAULT 0,
                        revolver_assists INT DEFAULT 0,
                        nova_assists INT DEFAULT 0,
                        xm1014_assists INT DEFAULT 0,
                        sawedoff_assists INT DEFAULT 0,
                        mag7_assists INT DEFAULT 0,
                        mac10_assists INT DEFAULT 0,
                        mp9_assists INT DEFAULT 0,
                        mp7_assists INT DEFAULT 0,
                        ump45_assists INT DEFAULT 0,
                        p90_assists INT DEFAULT 0,
                        bizon_assists INT DEFAULT 0,
                        negev_assists INT DEFAULT 0,
                        m249_assists INT DEFAULT 0,
                        ak47_headshots INT DEFAULT 0,
                        m4a4_headshots INT DEFAULT 0,
                        m4a1s_headshots INT DEFAULT 0,
                        aug_headshots INT DEFAULT 0,
                        sg553_headshots INT DEFAULT 0,
                        galilar_headshots INT DEFAULT 0,
                        famas_headshots INT DEFAULT 0,
                        awp_headshots INT DEFAULT 0,
                        scout_headshots INT DEFAULT 0,
                        scar20_headshots INT DEFAULT 0,
                        g3sg1_headshots INT DEFAULT 0,
                        ssg08_headshots INT DEFAULT 0,
                        deagle_headshots INT DEFAULT 0,
                        glock_headshots INT DEFAULT 0,
                        usps_headshots INT DEFAULT 0,
                        p250_headshots INT DEFAULT 0,
                        p2000_headshots INT DEFAULT 0,
                        fiveseven_headshots INT DEFAULT 0,
                        tec9_headshots INT DEFAULT 0,
                        cz75_headshots INT DEFAULT 0,
                        dualies_headshots INT DEFAULT 0,
                        revolver_headshots INT DEFAULT 0,
                        nova_headshots INT DEFAULT 0,
                        xm1014_headshots INT DEFAULT 0,
                        sawedoff_headshots INT DEFAULT 0,
                        mag7_headshots INT DEFAULT 0,
                        mac10_headshots INT DEFAULT 0,
                        mp9_headshots INT DEFAULT 0,
                        mp7_headshots INT DEFAULT 0,
                        ump45_headshots INT DEFAULT 0,
                        p90_headshots INT DEFAULT 0,
                        bizon_headshots INT DEFAULT 0,
                        negev_headshots INT DEFAULT 0,
                        m249_headshots INT DEFAULT 0,
                        ak47_chestshots INT DEFAULT 0,
                        m4a4_chestshots INT DEFAULT 0,
                        m4a1s_chestshots INT DEFAULT 0,
                        aug_chestshots INT DEFAULT 0,
                        sg553_chestshots INT DEFAULT 0,
                        galilar_chestshots INT DEFAULT 0,
                        famas_chestshots INT DEFAULT 0,
                        awp_chestshots INT DEFAULT 0,
                        scout_chestshots INT DEFAULT 0,
                        scar20_chestshots INT DEFAULT 0,
                        g3sg1_chestshots INT DEFAULT 0,
                        ssg08_chestshots INT DEFAULT 0,
                        deagle_chestshots INT DEFAULT 0,
                        glock_chestshots INT DEFAULT 0,
                        usps_chestshots INT DEFAULT 0,
                        p250_chestshots INT DEFAULT 0,
                        p2000_chestshots INT DEFAULT 0,
                        fiveseven_chestshots INT DEFAULT 0,
                        tec9_chestshots INT DEFAULT 0,
                        cz75_chestshots INT DEFAULT 0,
                        dualies_chestshots INT DEFAULT 0,
                        revolver_chestshots INT DEFAULT 0,
                        nova_chestshots INT DEFAULT 0,
                        xm1014_chestshots INT DEFAULT 0,
                        sawedoff_chestshots INT DEFAULT 0,
                        mag7_chestshots INT DEFAULT 0,
                        mac10_chestshots INT DEFAULT 0,
                        mp9_chestshots INT DEFAULT 0,
                        mp7_chestshots INT DEFAULT 0,
                        ump45_chestshots INT DEFAULT 0,
                        p90_chestshots INT DEFAULT 0,
                        bizon_chestshots INT DEFAULT 0,
                        negev_chestshots INT DEFAULT 0,
                        m249_chestshots INT DEFAULT 0,
                        ak47_stomachshots INT DEFAULT 0,
                        m4a4_stomachshots INT DEFAULT 0,
                        m4a1s_stomachshots INT DEFAULT 0,
                        aug_stomachshots INT DEFAULT 0,
                        sg553_stomachshots INT DEFAULT 0,
                        galilar_stomachshots INT DEFAULT 0,
                        famas_stomachshots INT DEFAULT 0,
                        awp_stomachshots INT DEFAULT 0,
                        scout_stomachshots INT DEFAULT 0,
                        scar20_stomachshots INT DEFAULT 0,
                        g3sg1_stomachshots INT DEFAULT 0,
                        ssg08_stomachshots INT DEFAULT 0,
                        deagle_stomachshots INT DEFAULT 0,
                        glock_stomachshots INT DEFAULT 0,
                        usps_stomachshots INT DEFAULT 0,
                        p250_stomachshots INT DEFAULT 0,
                        p2000_stomachshots INT DEFAULT 0,
                        fiveseven_stomachshots INT DEFAULT 0,
                        tec9_stomachshots INT DEFAULT 0,
                        cz75_stomachshots INT DEFAULT 0,
                        dualies_stomachshots INT DEFAULT 0,
                        revolver_stomachshots INT DEFAULT 0,
                        nova_stomachshots INT DEFAULT 0,
                        xm1014_stomachshots INT DEFAULT 0,
                        sawedoff_stomachshots INT DEFAULT 0,
                        mag7_stomachshots INT DEFAULT 0,
                        mac10_stomachshots INT DEFAULT 0,
                        mp9_stomachshots INT DEFAULT 0,
                        mp7_stomachshots INT DEFAULT 0,
                        ump45_stomachshots INT DEFAULT 0,
                        p90_stomachshots INT DEFAULT 0,
                        bizon_stomachshots INT DEFAULT 0,
                        negev_stomachshots INT DEFAULT 0,
                        m249_stomachshots INT DEFAULT 0,
                        ak47_leftlegshots INT DEFAULT 0,
                        m4a4_leftlegshots INT DEFAULT 0,
                        m4a1s_leftlegshots INT DEFAULT 0,
                        aug_leftlegshots INT DEFAULT 0,
                        sg553_leftlegshots INT DEFAULT 0,
                        galilar_leftlegshots INT DEFAULT 0,
                        famas_leftlegshots INT DEFAULT 0,
                        awp_leftlegshots INT DEFAULT 0,
                        scout_leftlegshots INT DEFAULT 0,
                        scar20_leftlegshots INT DEFAULT 0,
                        g3sg1_leftlegshots INT DEFAULT 0,
                        ssg08_leftlegshots INT DEFAULT 0,
                        deagle_leftlegshots INT DEFAULT 0,
                        glock_leftlegshots INT DEFAULT 0,
                        usps_leftlegshots INT DEFAULT 0,
                        p250_leftlegshots INT DEFAULT 0,
                        p2000_leftlegshots INT DEFAULT 0,
                        fiveseven_leftlegshots INT DEFAULT 0,
                        tec9_leftlegshots INT DEFAULT 0,
                        cz75_leftlegshots INT DEFAULT 0,
                        dualies_leftlegshots INT DEFAULT 0,
                        revolver_leftlegshots INT DEFAULT 0,
                        nova_leftlegshots INT DEFAULT 0,
                        xm1014_leftlegshots INT DEFAULT 0,
                        sawedoff_leftlegshots INT DEFAULT 0,
                        mag7_leftlegshots INT DEFAULT 0,
                        mac10_leftlegshots INT DEFAULT 0,
                        mp9_leftlegshots INT DEFAULT 0,
                        mp7_leftlegshots INT DEFAULT 0,
                        ump45_leftlegshots INT DEFAULT 0,
                        p90_leftlegshots INT DEFAULT 0,
                        bizon_leftlegshots INT DEFAULT 0,
                        negev_leftlegshots INT DEFAULT 0,
                        m249_leftlegshots INT DEFAULT 0,
                        ak47_rightlegshots INT DEFAULT 0,
                        m4a4_rightlegshots INT DEFAULT 0,
                        m4a1s_rightlegshots INT DEFAULT 0,
                        aug_rightlegshots INT DEFAULT 0,
                        sg553_rightlegshots INT DEFAULT 0,
                        galilar_rightlegshots INT DEFAULT 0,
                        famas_rightlegshots INT DEFAULT 0,
                        awp_rightlegshots INT DEFAULT 0,
                        scout_rightlegshots INT DEFAULT 0,
                        scar20_rightlegshots INT DEFAULT 0,
                        g3sg1_rightlegshots INT DEFAULT 0,
                        ssg08_rightlegshots INT DEFAULT 0,
                        deagle_rightlegshots INT DEFAULT 0,
                        glock_rightlegshots INT DEFAULT 0,
                        usps_rightlegshots INT DEFAULT 0,
                        p250_rightlegshots INT DEFAULT 0,
                        p2000_rightlegshots INT DEFAULT 0,
                        fiveseven_rightlegshots INT DEFAULT 0,
                        tec9_rightlegshots INT DEFAULT 0,
                        cz75_rightlegshots INT DEFAULT 0,
                        dualies_rightlegshots INT DEFAULT 0,
                        revolver_rightlegshots INT DEFAULT 0,
                        nova_rightlegshots INT DEFAULT 0,
                        xm1014_rightlegshots INT DEFAULT 0,
                        sawedoff_rightlegshots INT DEFAULT 0,
                        mag7_rightlegshots INT DEFAULT 0,
                        mac10_rightlegshots INT DEFAULT 0,
                        mp9_rightlegshots INT DEFAULT 0,
                        mp7_rightlegshots INT DEFAULT 0,
                        ump45_rightlegshots INT DEFAULT 0,
                        p90_rightlegshots INT DEFAULT 0,
                        bizon_rightlegshots INT DEFAULT 0,
                        negev_rightlegshots INT DEFAULT 0,
                        m249_rightlegshots INT DEFAULT 0,
                        last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        INDEX idx_steam_id (steam_id)
                    )";

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(createTableQuery, connection);
                await command.ExecuteNonQueryAsync();
                
                _logger?.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to initialize database: {ex.Message}");
                throw;
            }
        }

        public async Task<PlayerStats?> GetPlayerStatsAsync(string steamId)
        {
            try
            {
                string query = @"
                    SELECT * FROM player_stats WHERE steam_id = @steamId";

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@steamId", steamId);
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new PlayerStats
                    {
                        Id = reader.GetInt32("id"),
                        SteamId = reader.GetString("steam_id"),
                        PlayerName = reader.GetString("player_name"),
                        TotalKills = reader.GetInt32("total_kills"),
                        TotalDeaths = reader.GetInt32("total_deaths"),
                        TotalAssists = reader.GetInt32("total_assists"),
                        TotalHeadshots = reader.GetInt32("total_headshots"),
                        TotalRoundsPlayed = reader.GetInt32("total_rounds_played"),
                        TotalDamageDone = reader.GetInt32("total_damage_done"),
                        TotalDamageTaken = reader.GetInt32("total_damage_taken"),
                        AK47Kills = reader.GetInt32("ak47_kills"),
                        M4A4Kills = reader.GetInt32("m4a4_kills"),
                        M4A1SKills = reader.GetInt32("m4a1s_kills"),
                        AUGKills = reader.GetInt32("aug_kills"),
                        SG553Kills = reader.GetInt32("sg553_kills"),
                        GalilARKills = reader.GetInt32("galilar_kills"),
                        FAMASKills = reader.GetInt32("famas_kills"),
                        AWPKills = reader.GetInt32("awp_kills"),
                        ScoutKills = reader.GetInt32("scout_kills"),
                        Scar20Kills = reader.GetInt32("scar20_kills"),
                        G3SG1Kills = reader.GetInt32("g3sg1_kills"),
                        SSG08Kills = reader.GetInt32("ssg08_kills"),
                        DeagleKills = reader.GetInt32("deagle_kills"),
                        GlockKills = reader.GetInt32("glock_kills"),
                        USPSKills = reader.GetInt32("usps_kills"),
                        P250Kills = reader.GetInt32("p250_kills"),
                        P2000Kills = reader.GetInt32("p2000_kills"),
                        FiveSevenKills = reader.GetInt32("fiveseven_kills"),
                        Tec9Kills = reader.GetInt32("tec9_kills"),
                        CZ75Kills = reader.GetInt32("cz75_kills"),
                        DualiesKills = reader.GetInt32("dualies_kills"),
                        RevolverKills = reader.GetInt32("revolver_kills"),
                        NovaKills = reader.GetInt32("nova_kills"),
                        XM1014Kills = reader.GetInt32("xm1014_kills"),
                        SawedOffKills = reader.GetInt32("sawedoff_kills"),
                        MAG7Kills = reader.GetInt32("mag7_kills"),
                        Mac10Kills = reader.GetInt32("mac10_kills"),
                        MP9Kills = reader.GetInt32("mp9_kills"),
                        MP7Kills = reader.GetInt32("mp7_kills"),
                        UMP45Kills = reader.GetInt32("ump45_kills"),
                        P90Kills = reader.GetInt32("p90_kills"),
                        BizonKills = reader.GetInt32("bizon_kills"),
                        NegevKills = reader.GetInt32("negev_kills"),
                        M249Kills = reader.GetInt32("m249_kills"),
                        AK47Deaths = reader.GetInt32("ak47_deaths"),
                        M4A4Deaths = reader.GetInt32("m4a4_deaths"),
                        M4A1SDeaths = reader.GetInt32("m4a1s_deaths"),
                        AUGDeaths = reader.GetInt32("aug_deaths"),
                        SG553Deaths = reader.GetInt32("sg553_deaths"),
                        GalilARDeaths = reader.GetInt32("galilar_deaths"),
                        FAMASDeaths = reader.GetInt32("famas_deaths"),
                        AWPDeaths = reader.GetInt32("awp_deaths"),
                        ScoutDeaths = reader.GetInt32("scout_deaths"),
                        Scar20Deaths = reader.GetInt32("scar20_deaths"),
                        G3SG1Deaths = reader.GetInt32("g3sg1_deaths"),
                        SSG08Deaths = reader.GetInt32("ssg08_deaths"),
                        DeagleDeaths = reader.GetInt32("deagle_deaths"),
                        GlockDeaths = reader.GetInt32("glock_deaths"),
                        USPSDeaths = reader.GetInt32("usps_deaths"),
                        P250Deaths = reader.GetInt32("p250_deaths"),
                        P2000Deaths = reader.GetInt32("p2000_deaths"),
                        FiveSevenDeaths = reader.GetInt32("fiveseven_deaths"),
                        Tec9Deaths = reader.GetInt32("tec9_deaths"),
                        CZ75Deaths = reader.GetInt32("cz75_deaths"),
                        DualiesDeaths = reader.GetInt32("dualies_deaths"),
                        RevolverDeaths = reader.GetInt32("revolver_deaths"),
                        NovaDeaths = reader.GetInt32("nova_deaths"),
                        XM1014Deaths = reader.GetInt32("xm1014_deaths"),
                        SawedOffDeaths = reader.GetInt32("sawedoff_deaths"),
                        MAG7Deaths = reader.GetInt32("mag7_deaths"),
                        Mac10Deaths = reader.GetInt32("mac10_deaths"),
                        MP9Deaths = reader.GetInt32("mp9_deaths"),
                        MP7Deaths = reader.GetInt32("mp7_deaths"),
                        UMP45Deaths = reader.GetInt32("ump45_deaths"),
                        P90Deaths = reader.GetInt32("p90_deaths"),
                        BizonDeaths = reader.GetInt32("bizon_deaths"),
                        NegevDeaths = reader.GetInt32("negev_deaths"),
                        M249Deaths = reader.GetInt32("m249_deaths"),
                        AK47Assists = reader.GetInt32("ak47_assists"),
                        M4A4Assists = reader.GetInt32("m4a4_assists"),
                        M4A1SAssists = reader.GetInt32("m4a1s_assists"),
                        AUGAssists = reader.GetInt32("aug_assists"),
                        SG553Assists = reader.GetInt32("sg553_assists"),
                        GalilARAssists = reader.GetInt32("galilar_assists"),
                        FAMASAssists = reader.GetInt32("famas_assists"),
                        AWPAssists = reader.GetInt32("awp_assists"),
                        ScoutAssists = reader.GetInt32("scout_assists"),
                        Scar20Assists = reader.GetInt32("scar20_assists"),
                        G3SG1Assists = reader.GetInt32("g3sg1_assists"),
                        SSG08Assists = reader.GetInt32("ssg08_assists"),
                        DeagleAssists = reader.GetInt32("deagle_assists"),
                        GlockAssists = reader.GetInt32("glock_assists"),
                        USPSAssists = reader.GetInt32("usps_assists"),
                        P250Assists = reader.GetInt32("p250_assists"),
                        P2000Assists = reader.GetInt32("p2000_assists"),
                        FiveSevenAssists = reader.GetInt32("fiveseven_assists"),
                        Tec9Assists = reader.GetInt32("tec9_assists"),
                        CZ75Assists = reader.GetInt32("cz75_assists"),
                        DualiesAssists = reader.GetInt32("dualies_assists"),
                        RevolverAssists = reader.GetInt32("revolver_assists"),
                        NovaAssists = reader.GetInt32("nova_assists"),
                        XM1014Assists = reader.GetInt32("xm1014_assists"),
                        SawedOffAssists = reader.GetInt32("sawedoff_assists"),
                        MAG7Assists = reader.GetInt32("mag7_assists"),
                        Mac10Assists = reader.GetInt32("mac10_assists"),
                        MP9Assists = reader.GetInt32("mp9_assists"),
                        MP7Assists = reader.GetInt32("mp7_assists"),
                        UMP45Assists = reader.GetInt32("ump45_assists"),
                        P90Assists = reader.GetInt32("p90_assists"),
                        BizonAssists = reader.GetInt32("bizon_assists"),
                        NegevAssists = reader.GetInt32("negev_assists"),
                        M249Assists = reader.GetInt32("m249_assists"),
                        AK47Headshots = reader.GetInt32("ak47_headshots"),
                        M4A4Headshots = reader.GetInt32("m4a4_headshots"),
                        M4A1SHeadshots = reader.GetInt32("m4a1s_headshots"),
                        AUGHeadshots = reader.GetInt32("aug_headshots"),
                        SG553Headshots = reader.GetInt32("sg553_headshots"),
                        GalilARHeadshots = reader.GetInt32("galilar_headshots"),
                        FAMASHeadshots = reader.GetInt32("famas_headshots"),
                        AWPHeadshots = reader.GetInt32("awp_headshots"),
                        ScoutHeadshots = reader.GetInt32("scout_headshots"),
                        Scar20Headshots = reader.GetInt32("scar20_headshots"),
                        G3SG1Headshots = reader.GetInt32("g3sg1_headshots"),
                        SSG08Headshots = reader.GetInt32("ssg08_headshots"),
                        DeagleHeadshots = reader.GetInt32("deagle_headshots"),
                        GlockHeadshots = reader.GetInt32("glock_headshots"),
                        USPSHeadshots = reader.GetInt32("usps_headshots"),
                        P250Headshots = reader.GetInt32("p250_headshots"),
                        P2000Headshots = reader.GetInt32("p2000_headshots"),
                        FiveSevenHeadshots = reader.GetInt32("fiveseven_headshots"),
                        Tec9Headshots = reader.GetInt32("tec9_headshots"),
                        CZ75Headshots = reader.GetInt32("cz75_headshots"),
                        DualiesHeadshots = reader.GetInt32("dualies_headshots"),
                        RevolverHeadshots = reader.GetInt32("revolver_headshots"),
                        NovaHeadshots = reader.GetInt32("nova_headshots"),
                        XM1014Headshots = reader.GetInt32("xm1014_headshots"),
                        SawedOffHeadshots = reader.GetInt32("sawedoff_headshots"),
                        MAG7Headshots = reader.GetInt32("mag7_headshots"),
                        Mac10Headshots = reader.GetInt32("mac10_headshots"),
                        MP9Headshots = reader.GetInt32("mp9_headshots"),
                        MP7Headshots = reader.GetInt32("mp7_headshots"),
                        UMP45Headshots = reader.GetInt32("ump45_headshots"),
                        P90Headshots = reader.GetInt32("p90_headshots"),
                        BizonHeadshots = reader.GetInt32("bizon_headshots"),
                        NegevHeadshots = reader.GetInt32("negev_headshots"),
                        M249Headshots = reader.GetInt32("m249_headshots"),
                        AK47Chestshots = reader.GetInt32("ak47_chestshots"),
                        M4A4Chestshots = reader.GetInt32("m4a4_chestshots"),
                        M4A1SChestshots = reader.GetInt32("m4a1s_chestshots"),
                        AUGChestshots = reader.GetInt32("aug_chestshots"),
                        SG553Chestshots = reader.GetInt32("sg553_chestshots"),
                        GalilARChestshots = reader.GetInt32("galilar_chestshots"),
                        FAMASChestshots = reader.GetInt32("famas_chestshots"),
                        AWPChestshots = reader.GetInt32("awp_chestshots"),
                        ScoutChestshots = reader.GetInt32("scout_chestshots"),
                        Scar20Chestshots = reader.GetInt32("scar20_chestshots"),
                        G3SG1Chestshots = reader.GetInt32("g3sg1_chestshots"),
                        SSG08Chestshots = reader.GetInt32("ssg08_chestshots"),
                        DeagleChestshots = reader.GetInt32("deagle_chestshots"),
                        GlockChestshots = reader.GetInt32("glock_chestshots"),
                        USPSChestshots = reader.GetInt32("usps_chestshots"),
                        P250Chestshots = reader.GetInt32("p250_chestshots"),
                        P2000Chestshots = reader.GetInt32("p2000_chestshots"),
                        FiveSevenChestshots = reader.GetInt32("fiveseven_chestshots"),
                        Tec9Chestshots = reader.GetInt32("tec9_chestshots"),
                        CZ75Chestshots = reader.GetInt32("cz75_chestshots"),
                        DualiesChestshots = reader.GetInt32("dualies_chestshots"),
                        RevolverChestshots = reader.GetInt32("revolver_chestshots"),
                        NovaChestshots = reader.GetInt32("nova_chestshots"),
                        XM1014Chestshots = reader.GetInt32("xm1014_chestshots"),
                        SawedOffChestshots = reader.GetInt32("sawedoff_chestshots"),
                        MAG7Chestshots = reader.GetInt32("mag7_chestshots"),
                        Mac10Chestshots = reader.GetInt32("mac10_chestshots"),
                        MP9Chestshots = reader.GetInt32("mp9_chestshots"),
                        MP7Chestshots = reader.GetInt32("mp7_chestshots"),
                        UMP45Chestshots = reader.GetInt32("ump45_chestshots"),
                        P90Chestshots = reader.GetInt32("p90_chestshots"),
                        BizonChestshots = reader.GetInt32("bizon_chestshots"),
                        NegevChestshots = reader.GetInt32("negev_chestshots"),
                        M249Chestshots = reader.GetInt32("m249_chestshots"),
                        AK47Stomachshots = reader.GetInt32("ak47_stomachshots"),
                        M4A4Stomachshots = reader.GetInt32("m4a4_stomachshots"),
                        M4A1SStomachshots = reader.GetInt32("m4a1s_stomachshots"),
                        AUGStomachshots = reader.GetInt32("aug_stomachshots"),
                        SG553Stomachshots = reader.GetInt32("sg553_stomachshots"),
                        GalilARStomachshots = reader.GetInt32("galilar_stomachshots"),
                        FAMASStomachshots = reader.GetInt32("famas_stomachshots"),
                        AWPStomachshots = reader.GetInt32("awp_stomachshots"),
                        ScoutStomachshots = reader.GetInt32("scout_stomachshots"),
                        Scar20Stomachshots = reader.GetInt32("scar20_stomachshots"),
                        G3SG1Stomachshots = reader.GetInt32("g3sg1_stomachshots"),
                        SSG08Stomachshots = reader.GetInt32("ssg08_stomachshots"),
                        DeagleStomachshots = reader.GetInt32("deagle_stomachshots"),
                        GlockStomachshots = reader.GetInt32("glock_stomachshots"),
                        USPSStomachshots = reader.GetInt32("usps_stomachshots"),
                        P250Stomachshots = reader.GetInt32("p250_stomachshots"),
                        P2000Stomachshots = reader.GetInt32("p2000_stomachshots"),
                        FiveSevenStomachshots = reader.GetInt32("fiveseven_stomachshots"),
                        Tec9Stomachshots = reader.GetInt32("tec9_stomachshots"),
                        CZ75Stomachshots = reader.GetInt32("cz75_stomachshots"),
                        DualiesStomachshots = reader.GetInt32("dualies_stomachshots"),
                        RevolverStomachshots = reader.GetInt32("revolver_stomachshots"),
                        NovaStomachshots = reader.GetInt32("nova_stomachshots"),
                        XM1014Stomachshots = reader.GetInt32("xm1014_stomachshots"),
                        SawedOffStomachshots = reader.GetInt32("sawedoff_stomachshots"),
                        MAG7Stomachshots = reader.GetInt32("mag7_stomachshots"),
                        Mac10Stomachshots = reader.GetInt32("mac10_stomachshots"),
                        MP9Stomachshots = reader.GetInt32("mp9_stomachshots"),
                        MP7Stomachshots = reader.GetInt32("mp7_stomachshots"),
                        UMP45Stomachshots = reader.GetInt32("ump45_stomachshots"),
                        P90Stomachshots = reader.GetInt32("p90_stomachshots"),
                        BizonStomachshots = reader.GetInt32("bizon_stomachshots"),
                        NegevStomachshots = reader.GetInt32("negev_stomachshots"),
                        M249Stomachshots = reader.GetInt32("m249_stomachshots"),
                        AK47LeftLegshots = reader.GetInt32("ak47_leftlegshots"),
                        M4A4LeftLegshots = reader.GetInt32("m4a4_leftlegshots"),
                        M4A1SLeftLegshots = reader.GetInt32("m4a1s_leftlegshots"),
                        AUGLeftLegshots = reader.GetInt32("aug_leftlegshots"),
                        SG553LeftLegshots = reader.GetInt32("sg553_leftlegshots"),
                        GalilARLeftLegshots = reader.GetInt32("galilar_leftlegshots"),
                        FAMASLeftLegshots = reader.GetInt32("famas_leftlegshots"),
                        AWPLeftLegshots = reader.GetInt32("awp_leftlegshots"),
                        ScoutLeftLegshots = reader.GetInt32("scout_leftlegshots"),
                        Scar20LeftLegshots = reader.GetInt32("scar20_leftlegshots"),
                        G3SG1LeftLegshots = reader.GetInt32("g3sg1_leftlegshots"),
                        SSG08LeftLegshots = reader.GetInt32("ssg08_leftlegshots"),
                        DeagleLeftLegshots = reader.GetInt32("deagle_leftlegshots"),
                        GlockLeftLegshots = reader.GetInt32("glock_leftlegshots"),
                        USPSLeftLegshots = reader.GetInt32("usps_leftlegshots"),
                        P250LeftLegshots = reader.GetInt32("p250_leftlegshots"),
                        P2000LeftLegshots = reader.GetInt32("p2000_leftlegshots"),
                        FiveSevenLeftLegshots = reader.GetInt32("fiveseven_leftlegshots"),
                        Tec9LeftLegshots = reader.GetInt32("tec9_leftlegshots"),
                        CZ75LeftLegshots = reader.GetInt32("cz75_leftlegshots"),
                        DualiesLeftLegshots = reader.GetInt32("dualies_leftlegshots"),
                        RevolverLeftLegshots = reader.GetInt32("revolver_leftlegshots"),
                        NovaLeftLegshots = reader.GetInt32("nova_leftlegshots"),
                        XM1014LeftLegshots = reader.GetInt32("xm1014_leftlegshots"),
                        SawedOffLeftLegshots = reader.GetInt32("sawedoff_leftlegshots"),
                        MAG7LeftLegshots = reader.GetInt32("mag7_leftlegshots"),
                        Mac10LeftLegshots = reader.GetInt32("mac10_leftlegshots"),
                        MP9LeftLegshots = reader.GetInt32("mp9_leftlegshots"),
                        MP7LeftLegshots = reader.GetInt32("mp7_leftlegshots"),
                        UMP45LeftLegshots = reader.GetInt32("ump45_leftlegshots"),
                        P90LeftLegshots = reader.GetInt32("p90_leftlegshots"),
                        BizonLeftLegshots = reader.GetInt32("bizon_leftlegshots"),
                        NegevLeftLegshots = reader.GetInt32("negev_leftlegshots"),
                        M249LeftLegshots = reader.GetInt32("m249_leftlegshots"),
                        AK47RightLegshots = reader.GetInt32("ak47_rightlegshots"),
                        M4A4RightLegshots = reader.GetInt32("m4a4_rightlegshots"),
                        M4A1SRightLegshots = reader.GetInt32("m4a1s_rightlegshots"),
                        AUGRightLegshots = reader.GetInt32("aug_rightlegshots"),
                        SG553RightLegshots = reader.GetInt32("sg553_rightlegshots"),
                        GalilARRightLegshots = reader.GetInt32("galilar_rightlegshots"),
                        FAMASRightLegshots = reader.GetInt32("famas_rightlegshots"),
                        AWPRightLegshots = reader.GetInt32("awp_rightlegshots"),
                        ScoutRightLegshots = reader.GetInt32("scout_rightlegshots"),
                        Scar20RightLegshots = reader.GetInt32("scar20_rightlegshots"),
                        G3SG1RightLegshots = reader.GetInt32("g3sg1_rightlegshots"),
                        SSG08RightLegshots = reader.GetInt32("ssg08_rightlegshots"),
                        DeagleRightLegshots = reader.GetInt32("deagle_rightlegshots"),
                        GlockRightLegshots = reader.GetInt32("glock_rightlegshots"),
                        USPSRightLegshots = reader.GetInt32("usps_rightlegshots"),
                        P250RightLegshots = reader.GetInt32("p250_rightlegshots"),
                        P2000RightLegshots = reader.GetInt32("p2000_rightlegshots"),
                        FiveSevenRightLegshots = reader.GetInt32("fiveseven_rightlegshots"),
                        Tec9RightLegshots = reader.GetInt32("tec9_rightlegshots"),
                        CZ75RightLegshots = reader.GetInt32("cz75_rightlegshots"),
                        DualiesRightLegshots = reader.GetInt32("dualies_rightlegshots"),
                        RevolverRightLegshots = reader.GetInt32("revolver_rightlegshots"),
                        NovaRightLegshots = reader.GetInt32("nova_rightlegshots"),
                        XM1014RightLegshots = reader.GetInt32("xm1014_rightlegshots"),
                        SawedOffRightLegshots = reader.GetInt32("sawedoff_rightlegshots"),
                        MAG7RightLegshots = reader.GetInt32("mag7_rightlegshots"),
                        Mac10RightLegshots = reader.GetInt32("mac10_rightlegshots"),
                        MP9RightLegshots = reader.GetInt32("mp9_rightlegshots"),
                        MP7RightLegshots = reader.GetInt32("mp7_rightlegshots"),
                        UMP45RightLegshots = reader.GetInt32("ump45_rightlegshots"),
                        P90RightLegshots = reader.GetInt32("p90_rightlegshots"),
                        BizonRightLegshots = reader.GetInt32("bizon_rightlegshots"),
                        NegevRightLegshots = reader.GetInt32("negev_rightlegshots"),
                        M249RightLegshots = reader.GetInt32("m249_rightlegshots"),
                        LastUpdated = reader.GetDateTime("last_updated")
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to get player stats: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdatePlayerStatsAsync(PlayerStats stats)
        {
            try
            {
                string query = @"
                    INSERT INTO player_stats (
                        steam_id, player_name, total_kills, total_deaths, total_assists,
                        total_headshots, total_rounds_played, total_damage_done, total_damage_taken,
                        ak47_kills, m4a4_kills, m4a1s_kills, aug_kills, sg553_kills,
                        galilar_kills, famas_kills, awp_kills, scout_kills, scar20_kills,
                        g3sg1_kills, ssg08_kills, deagle_kills, glock_kills, usps_kills,
                        p250_kills, p2000_kills, fiveseven_kills, tec9_kills, cz75_kills,
                        dualies_kills, revolver_kills, nova_kills, xm1014_kills, sawedoff_kills,
                        mag7_kills, mac10_kills, mp9_kills, mp7_kills, ump45_kills,
                        p90_kills, bizon_kills, negev_kills, m249_kills,
                        ak47_deaths, m4a4_deaths, m4a1s_deaths, aug_deaths, sg553_deaths,
                        galilar_deaths, famas_deaths, awp_deaths, scout_deaths, scar20_deaths,
                        g3sg1_deaths, ssg08_deaths, deagle_deaths, glock_deaths, usps_deaths,
                        p250_deaths, p2000_deaths, fiveseven_deaths, tec9_deaths, cz75_deaths,
                        dualies_deaths, revolver_deaths, nova_deaths, xm1014_deaths, sawedoff_deaths,
                        mag7_deaths, mac10_deaths, mp9_deaths, mp7_deaths, ump45_deaths,
                        p90_deaths, bizon_deaths, negev_deaths, m249_deaths,
                        ak47_assists, m4a4_assists, m4a1s_assists, aug_assists, sg553_assists,
                        galilar_assists, famas_assists, awp_assists, scout_assists, scar20_assists,
                        g3sg1_assists, ssg08_assists, deagle_assists, glock_assists, usps_assists,
                        p250_assists, p2000_assists, fiveseven_assists, tec9_assists, cz75_assists,
                        dualies_assists, revolver_assists, nova_assists, xm1014_assists, sawedoff_assists,
                        mag7_assists, mac10_assists, mp9_assists, mp7_assists, ump45_assists,
                        p90_assists, bizon_assists, negev_assists, m249_assists,
                        ak47_headshots, m4a4_headshots, m4a1s_headshots, aug_headshots, sg553_headshots,
                        galilar_headshots, famas_headshots, awp_headshots, scout_headshots, scar20_headshots,
                        g3sg1_headshots, ssg08_headshots, deagle_headshots, glock_headshots, usps_headshots,
                        p250_headshots, p2000_headshots, fiveseven_headshots, tec9_headshots, cz75_headshots,
                        dualies_headshots, revolver_headshots, nova_headshots, xm1014_headshots, sawedoff_headshots,
                        mag7_headshots, mac10_headshots, mp9_headshots, mp7_headshots, ump45_headshots,
                        p90_headshots, bizon_headshots, negev_headshots, m249_headshots,
                        ak47_chestshots, m4a4_chestshots, m4a1s_chestshots, aug_chestshots, sg553_chestshots,
                        galilar_chestshots, famas_chestshots, awp_chestshots, scout_chestshots, scar20_chestshots,
                        g3sg1_chestshots, ssg08_chestshots, deagle_chestshots, glock_chestshots, usps_chestshots,
                        p250_chestshots, p2000_chestshots, fiveseven_chestshots, tec9_chestshots, cz75_chestshots,
                        dualies_chestshots, revolver_chestshots, nova_chestshots, xm1014_chestshots, sawedoff_chestshots,
                        mag7_chestshots, mac10_chestshots, mp9_chestshots, mp7_chestshots, ump45_chestshots,
                        p90_chestshots, bizon_chestshots, negev_chestshots, m249_chestshots,
                        ak47_stomachshots, m4a4_stomachshots, m4a1s_stomachshots, aug_stomachshots, sg553_stomachshots,
                        galilar_stomachshots, famas_stomachshots, awp_stomachshots, scout_stomachshots, scar20_stomachshots,
                        g3sg1_stomachshots, ssg08_stomachshots, deagle_stomachshots, glock_stomachshots, usps_stomachshots,
                        p250_stomachshots, p2000_stomachshots, fiveseven_stomachshots, tec9_stomachshots, cz75_stomachshots,
                        dualies_stomachshots, revolver_stomachshots, nova_stomachshots, xm1014_stomachshots, sawedoff_stomachshots,
                        mag7_stomachshots, mac10_stomachshots, mp9_stomachshots, mp7_stomachshots, ump45_stomachshots,
                        p90_stomachshots, bizon_stomachshots, negev_stomachshots, m249_stomachshots,
                        ak47_leftlegshots, m4a4_leftlegshots, m4a1s_leftlegshots, aug_leftlegshots, sg553_leftlegshots,
                        galilar_leftlegshots, famas_leftlegshots, awp_leftlegshots, scout_leftlegshots, scar20_leftlegshots,
                        g3sg1_leftlegshots, ssg08_leftlegshots, deagle_leftlegshots, glock_leftlegshots, usps_leftlegshots,
                        p250_leftlegshots, p2000_leftlegshots, fiveseven_leftlegshots, tec9_leftlegshots, cz75_leftlegshots,
                        dualies_leftlegshots, revolver_leftlegshots, nova_leftlegshots, xm1014_leftlegshots, sawedoff_leftlegshots,
                        mag7_leftlegshots, mac10_leftlegshots, mp9_leftlegshots, mp7_leftlegshots, ump45_leftlegshots,
                        p90_leftlegshots, bizon_leftlegshots, negev_leftlegshots, m249_leftlegshots,
                        ak47_rightlegshots, m4a4_rightlegshots, m4a1s_rightlegshots, aug_rightlegshots, sg553_rightlegshots,
                        galilar_rightlegshots, famas_rightlegshots, awp_rightlegshots, scout_rightlegshots, scar20_rightlegshots,
                        g3sg1_rightlegshots, ssg08_rightlegshots, deagle_rightlegshots, glock_rightlegshots, usps_rightlegshots,
                        p250_rightlegshots, p2000_rightlegshots, fiveseven_rightlegshots, tec9_rightlegshots, cz75_rightlegshots,
                        dualies_rightlegshots, revolver_rightlegshots, nova_rightlegshots, xm1014_rightlegshots, sawedoff_rightlegshots,
                        mag7_rightlegshots, mac10_rightlegshots, mp9_rightlegshots, mp7_rightlegshots, ump45_rightlegshots,
                        p90_rightlegshots, bizon_rightlegshots, negev_rightlegshots, m249_rightlegshots
                    ) VALUES (
                        @steamId, @playerName, @totalKills, @totalDeaths, @totalAssists,
                        @totalHeadshots, @totalRoundsPlayed, @totalDamageDone, @totalDamageTaken,
                        @ak47Kills, @m4a4Kills, @m4a1sKills, @augKills, @sg553Kills,
                        @galilarKills, @famasKills, @awpKills, @scoutKills, @scar20Kills,
                        @g3sg1Kills, @ssg08Kills, @deagleKills, @glockKills, @uspsKills,
                        @p250Kills, @p2000Kills, @fivesevenKills, @tec9Kills, @cz75Kills,
                        @dualiesKills, @revolverKills, @novaKills, @xm1014Kills, @sawedoffKills,
                        @mag7Kills, @mac10Kills, @mp9Kills, @mp7Kills, @ump45Kills,
                        @p90Kills, @bizonKills, @negevKills, @m249Kills,
                        @ak47Deaths, @m4a4Deaths, @m4a1sDeaths, @augDeaths, @sg553Deaths,
                        @galilarDeaths, @famasDeaths, @awpDeaths, @scoutDeaths, @scar20Deaths,
                        @g3sg1Deaths, @ssg08Deaths, @deagleDeaths, @glockDeaths, @uspsDeaths,
                        @p250Deaths, @p2000Deaths, @fivesevenDeaths, @tec9Deaths, @cz75Deaths,
                        @dualiesDeaths, @revolverDeaths, @novaDeaths, @xm1014Deaths, @sawedoffDeaths,
                        @mag7Deaths, @mac10Deaths, @mp9Deaths, @mp7Deaths, @ump45Deaths,
                        @p90Deaths, @bizonDeaths, @negevDeaths, @m249Deaths,
                        @ak47Assists, @m4a4Assists, @m4a1sAssists, @augAssists, @sg553Assists,
                        @galilarAssists, @famasAssists, @awpAssists, @scoutAssists, @scar20Assists,
                        @g3sg1Assists, @ssg08Assists, @deagleAssists, @glockAssists, @uspsAssists,
                        @p250Assists, @p2000Assists, @fivesevenAssists, @tec9Assists, @cz75Assists,
                        @dualiesAssists, @revolverAssists, @novaAssists, @xm1014Assists, @sawedoffAssists,
                        @mag7Assists, @mac10Assists, @mp9Assists, @mp7Assists, @ump45Assists,
                        @p90Assists, @bizonAssists, @negevAssists, @m249Assists,
                        @ak47Headshots, @m4a4Headshots, @m4a1sHeadshots, @augHeadshots, @sg553Headshots,
                        @galilarHeadshots, @famasHeadshots, @awpHeadshots, @scoutHeadshots, @scar20Headshots,
                        @g3sg1Headshots, @ssg08Headshots, @deagleHeadshots, @glockHeadshots, @uspsHeadshots,
                        @p250Headshots, @p2000Headshots, @fivesevenHeadshots, @tec9Headshots, @cz75Headshots,
                        @dualiesHeadshots, @revolverHeadshots, @novaHeadshots, @xm1014Headshots, @sawedoffHeadshots,
                        @mag7Headshots, @mac10Headshots, @mp9Headshots, @mp7Headshots, @ump45Headshots,
                        @p90Headshots, @bizonHeadshots, @negevHeadshots, @m249Headshots,
                        @ak47Chestshots, @m4a4Chestshots, @m4a1sChestshots, @augChestshots, @sg553Chestshots,
                        @galilarChestshots, @famasChestshots, @awpChestshots, @scoutChestshots, @scar20Chestshots,
                        @g3sg1Chestshots, @ssg08Chestshots, @deagleChestshots, @glockChestshots, @uspsChestshots,
                        @p250Chestshots, @p2000Chestshots, @fivesevenChestshots, @tec9Chestshots, @cz75Chestshots,
                        @dualiesChestshots, @revolverChestshots, @novaChestshots, @xm1014Chestshots, @sawedoffChestshots,
                        @mag7Chestshots, @mac10Chestshots, @mp9Chestshots, @mp7Chestshots, @ump45Chestshots,
                        @p90Chestshots, @bizonChestshots, @negevChestshots, @m249Chestshots,
                        @ak47Stomachshots, @m4a4Stomachshots, @m4a1sStomachshots, @augStomachshots, @sg553Stomachshots,
                        @galilarStomachshots, @famasStomachshots, @awpStomachshots, @scoutStomachshots, @scar20Stomachshots,
                        @g3sg1Stomachshots, @ssg08Stomachshots, @deagleStomachshots, @glockStomachshots, @uspsStomachshots,
                        @p250Stomachshots, @p2000Stomachshots, @fivesevenStomachshots, @tec9Stomachshots, @cz75Stomachshots,
                        @dualiesStomachshots, @revolverStomachshots, @novaStomachshots, @xm1014Stomachshots, @sawedoffStomachshots,
                        @mag7Stomachshots, @mac10Stomachshots, @mp9Stomachshots, @mp7Stomachshots, @ump45Stomachshots,
                        @p90Stomachshots, @bizonStomachshots, @negevStomachshots, @m249Stomachshots,
                        @ak47LeftLegshots, @m4a4LeftLegshots, @m4a1sLeftLegshots, @augLeftLegshots, @sg553LeftLegshots,
                        @galilarLeftLegshots, @famasLeftLegshots, @awpLeftLegshots, @scoutLeftLegshots, @scar20LeftLegshots,
                        @g3sg1LeftLegshots, @ssg08LeftLegshots, @deagleLeftLegshots, @glockLeftLegshots, @uspsLeftLegshots,
                        @p250LeftLegshots, @p2000LeftLegshots, @fivesevenLeftLegshots, @tec9LeftLegshots, @cz75LeftLegshots,
                        @dualiesLeftLegshots, @revolverLeftLegshots, @novaLeftLegshots, @xm1014LeftLegshots, @sawedoffLeftLegshots,
                        @mag7LeftLegshots, @mac10LeftLegshots, @mp9LeftLegshots, @mp7LeftLegshots, @ump45LeftLegshots,
                        @p90LeftLegshots, @bizonLeftLegshots, @negevLeftLegshots, @m249LeftLegshots,
                        @ak47RightLegshots, @m4a4RightLegshots, @m4a1sRightLegshots, @augRightLegshots, @sg553RightLegshots,
                        @galilarRightLegshots, @famasRightLegshots, @awpRightLegshots, @scoutRightLegshots, @scar20RightLegshots,
                        @g3sg1RightLegshots, @ssg08RightLegshots, @deagleRightLegshots, @glockRightLegshots, @uspsRightLegshots,
                        @p250RightLegshots, @p2000RightLegshots, @fivesevenRightLegshots, @tec9RightLegshots, @cz75RightLegshots,
                        @dualiesRightLegshots, @revolverRightLegshots, @novaRightLegshots, @xm1014RightLegshots, @sawedoffRightLegshots,
                        @mag7RightLegshots, @mac10RightLegshots, @mp9RightLegshots, @mp7RightLegshots, @ump45RightLegshots,
                        @p90RightLegshots, @bizonRightLegshots, @negevRightLegshots, @m249RightLegshots
                    ) ON DUPLICATE KEY UPDATE
                        player_name = @playerName,
                        total_kills = total_kills + @totalKills,
                        total_deaths = total_deaths + @totalDeaths,
                        total_assists = total_assists + @totalAssists,
                        total_headshots = total_headshots + @totalHeadshots,
                        total_rounds_played = total_rounds_played + @totalRoundsPlayed,
                        total_damage_done = total_damage_done + @totalDamageDone,
                        total_damage_taken = total_damage_taken + @totalDamageTaken,
                        ak47_kills = ak47_kills + @ak47Kills,
                        m4a4_kills = m4a4_kills + @m4a4Kills,
                        m4a1s_kills = m4a1s_kills + @m4a1sKills,
                        aug_kills = aug_kills + @augKills,
                        sg553_kills = sg553_kills + @sg553Kills,
                        galilar_kills = galilar_kills + @galilarKills,
                        famas_kills = famas_kills + @famasKills,
                        awp_kills = awp_kills + @awpKills,
                        scout_kills = scout_kills + @scoutKills,
                        scar20_kills = scar20_kills + @scar20Kills,
                        g3sg1_kills = g3sg1_kills + @g3sg1Kills,
                        ssg08_kills = ssg08_kills + @ssg08Kills,
                        deagle_kills = deagle_kills + @deagleKills,
                        glock_kills = glock_kills + @glockKills,
                        usps_kills = usps_kills + @uspsKills,
                        p250_kills = p250_kills + @p250Kills,
                        p2000_kills = p2000_kills + @p2000Kills,
                        fiveseven_kills = fiveseven_kills + @fivesevenKills,
                        tec9_kills = tec9_kills + @tec9Kills,
                        cz75_kills = cz75_kills + @cz75Kills,
                        dualies_kills = dualies_kills + @dualiesKills,
                        revolver_kills = revolver_kills + @revolverKills,
                        nova_kills = nova_kills + @novaKills,
                        xm1014_kills = xm1014_kills + @xm1014Kills,
                        sawedoff_kills = sawedoff_kills + @sawedoffKills,
                        mag7_kills = mag7_kills + @mag7Kills,
                        mac10_kills = mac10_kills + @mac10Kills,
                        mp9_kills = mp9_kills + @mp9Kills,
                        mp7_kills = mp7_kills + @mp7Kills,
                        ump45_kills = ump45_kills + @ump45Kills,
                        p90_kills = p90_kills + @p90Kills,
                        bizon_kills = bizon_kills + @bizonKills,
                        negev_kills = negev_kills + @negevKills,
                        m249_kills = m249_kills + @m249Kills,
                        ak47_deaths = ak47_deaths + @ak47Deaths,
                        m4a4_deaths = m4a4_deaths + @m4a4Deaths,
                        m4a1s_deaths = m4a1s_deaths + @m4a1sDeaths,
                        aug_deaths = aug_deaths + @augDeaths,
                        sg553_deaths = sg553_deaths + @sg553Deaths,
                        galilar_deaths = galilar_deaths + @galilarDeaths,
                        famas_deaths = famas_deaths + @famasDeaths,
                        awp_deaths = awp_deaths + @awpDeaths,
                        scout_deaths = scout_deaths + @scoutDeaths,
                        scar20_deaths = scar20_deaths + @scar20Deaths,
                        g3sg1_deaths = g3sg1_deaths + @g3sg1Deaths,
                        ssg08_deaths = ssg08_deaths + @ssg08Deaths,
                        deagle_deaths = deagle_deaths + @deagleDeaths,
                        glock_deaths = glock_deaths + @glockDeaths,
                        usps_deaths = usps_deaths + @uspsDeaths,
                        p250_deaths = p250_deaths + @p250Deaths,
                        p2000_deaths = p2000_deaths + @p2000Deaths,
                        fiveseven_deaths = fiveseven_deaths + @fivesevenDeaths,
                        tec9_deaths = tec9_deaths + @tec9Deaths,
                        cz75_deaths = cz75_deaths + @cz75Deaths,
                        dualies_deaths = dualies_deaths + @dualiesDeaths,
                        revolver_deaths = revolver_deaths + @revolverDeaths,
                        nova_deaths = nova_deaths + @novaDeaths,
                        xm1014_deaths = xm1014_deaths + @xm1014Deaths,
                        sawedoff_deaths = sawedoff_deaths + @sawedoffDeaths,
                        mag7_deaths = mag7_deaths + @mag7Deaths,
                        mac10_deaths = mac10_deaths + @mac10Deaths,
                        mp9_deaths = mp9_deaths + @mp9Deaths,
                        mp7_deaths = mp7_deaths + @mp7Deaths,
                        ump45_deaths = ump45_deaths + @ump45Deaths,
                        p90_deaths = p90_deaths + @p90Deaths,
                        bizon_deaths = bizon_deaths + @bizonDeaths,
                        negev_deaths = negev_deaths + @negevDeaths,
                        m249_deaths = m249_deaths + @m249Deaths,
                        ak47_assists = ak47_assists + @ak47Assists,
                        m4a4_assists = m4a4_assists + @m4a4Assists,
                        m4a1s_assists = m4a1s_assists + @m4a1sAssists,
                        aug_assists = aug_assists + @augAssists,
                        sg553_assists = sg553_assists + @sg553Assists,
                        galilar_assists = galilar_assists + @galilarAssists,
                        famas_assists = famas_assists + @famasAssists,
                        awp_assists = awp_assists + @awpAssists,
                        scout_assists = scout_assists + @scoutAssists,
                        scar20_assists = scar20_assists + @scar20Assists,
                        g3sg1_assists = g3sg1_assists + @g3sg1Assists,
                        ssg08_assists = ssg08_assists + @ssg08Assists,
                        deagle_assists = deagle_assists + @deagleAssists,
                        glock_assists = glock_assists + @glockAssists,
                        usps_assists = usps_assists + @uspsAssists,
                        p250_assists = p250_assists + @p250Assists,
                        p2000_assists = p2000_assists + @p2000Assists,
                        fiveseven_assists = fiveseven_assists + @fivesevenAssists,
                        tec9_assists = tec9_assists + @tec9Assists,
                        cz75_assists = cz75_assists + @cz75Assists,
                        dualies_assists = dualies_assists + @dualiesAssists,
                        revolver_assists = revolver_assists + @revolverAssists,
                        nova_assists = nova_assists + @novaAssists,
                        xm1014_assists = xm1014_assists + @xm1014Assists,
                        sawedoff_assists = sawedoff_assists + @sawedoffAssists,
                        mag7_assists = mag7_assists + @mag7Assists,
                        mac10_assists = mac10_assists + @mac10Assists,
                        mp9_assists = mp9_assists + @mp9Assists,
                        mp7_assists = mp7_assists + @mp7Assists,
                        ump45_assists = ump45_assists + @ump45Assists,
                        p90_assists = p90_assists + @p90Assists,
                        bizon_assists = bizon_assists + @bizonAssists,
                        negev_assists = negev_assists + @negevAssists,
                        m249_assists = m249_assists + @m249Assists,
                        ak47_headshots = ak47_headshots + @ak47Headshots,
                        m4a4_headshots = m4a4_headshots + @m4a4Headshots,
                        m4a1s_headshots = m4a1s_headshots + @m4a1sHeadshots,
                        aug_headshots = aug_headshots + @augHeadshots,
                        sg553_headshots = sg553_headshots + @sg553Headshots,
                        galilar_headshots = galilar_headshots + @galilarHeadshots,
                        famas_headshots = famas_headshots + @famasHeadshots,
                        awp_headshots = awp_headshots + @awpHeadshots,
                        scout_headshots = scout_headshots + @scoutHeadshots,
                        scar20_headshots = scar20_headshots + @scar20Headshots,
                        g3sg1_headshots = g3sg1_headshots + @g3sg1Headshots,
                        ssg08_headshots = ssg08_headshots + @ssg08Headshots,
                        deagle_headshots = deagle_headshots + @deagleHeadshots,
                        glock_headshots = glock_headshots + @glockHeadshots,
                        usps_headshots = usps_headshots + @uspsHeadshots,
                        p250_headshots = p250_headshots + @p250Headshots,
                        p2000_headshots = p2000_headshots + @p2000Headshots,
                        fiveseven_headshots = fiveseven_headshots + @fivesevenHeadshots,
                        tec9_headshots = tec9_headshots + @tec9Headshots,
                        cz75_headshots = cz75_headshots + @cz75Headshots,
                        dualies_headshots = dualies_headshots + @dualiesHeadshots,
                        revolver_headshots = revolver_headshots + @revolverHeadshots,
                        nova_headshots = nova_headshots + @novaHeadshots,
                        xm1014_headshots = xm1014_headshots + @xm1014Headshots,
                        sawedoff_headshots = sawedoff_headshots + @sawedoffHeadshots,
                        mag7_headshots = mag7_headshots + @mag7Headshots,
                        mac10_headshots = mac10_headshots + @mac10Headshots,
                        mp9_headshots = mp9_headshots + @mp9Headshots,
                        mp7_headshots = mp7_headshots + @mp7Headshots,
                        ump45_headshots = ump45_headshots + @ump45Headshots,
                        p90_headshots = p90_headshots + @p90Headshots,
                        bizon_headshots = bizon_headshots + @bizonHeadshots,
                        negev_headshots = negev_headshots + @negevHeadshots,
                        m249_headshots = m249_headshots + @m249Headshots,
                        ak47_chestshots = ak47_chestshots + @ak47Chestshots,
                        m4a4_chestshots = m4a4_chestshots + @m4a4Chestshots,
                        m4a1s_chestshots = m4a1s_chestshots + @m4a1sChestshots,
                        aug_chestshots = aug_chestshots + @augChestshots,
                        sg553_chestshots = sg553_chestshots + @sg553Chestshots,
                        galilar_chestshots = galilar_chestshots + @galilarChestshots,
                        famas_chestshots = famas_chestshots + @famasChestshots,
                        awp_chestshots = awp_chestshots + @awpChestshots,
                        scout_chestshots = scout_chestshots + @scoutChestshots,
                        scar20_chestshots = scar20_chestshots + @scar20Chestshots,
                        g3sg1_chestshots = g3sg1_chestshots + @g3sg1Chestshots,
                        ssg08_chestshots = ssg08_chestshots + @ssg08Chestshots,
                        deagle_chestshots = deagle_chestshots + @deagleChestshots,
                        glock_chestshots = glock_chestshots + @glockChestshots,
                        usps_chestshots = usps_chestshots + @uspsChestshots,
                        p250_chestshots = p250_chestshots + @p250Chestshots,
                        p2000_chestshots = p2000_chestshots + @p2000Chestshots,
                        fiveseven_chestshots = fiveseven_chestshots + @fivesevenChestshots,
                        tec9_chestshots = tec9_chestshots + @tec9Chestshots,
                        cz75_chestshots = cz75_chestshots + @cz75Chestshots,
                        dualies_chestshots = dualies_chestshots + @dualiesChestshots,
                        revolver_chestshots = revolver_chestshots + @revolverChestshots,
                        nova_chestshots = nova_chestshots + @novaChestshots,
                        xm1014_chestshots = xm1014_chestshots + @xm1014Chestshots,
                        sawedoff_chestshots = sawedoff_chestshots + @sawedoffChestshots,
                        mag7_chestshots = mag7_chestshots + @mag7Chestshots,
                        mac10_chestshots = mac10_chestshots + @mac10Chestshots,
                        mp9_chestshots = mp9_chestshots + @mp9Chestshots,
                        mp7_chestshots = mp7_chestshots + @mp7Chestshots,
                        ump45_chestshots = ump45_chestshots + @ump45Chestshots,
                        p90_chestshots = p90_chestshots + @p90Chestshots,
                        bizon_chestshots = bizon_chestshots + @bizonChestshots,
                        negev_chestshots = negev_chestshots + @negevChestshots,
                        m249_chestshots = m249_chestshots + @m249Chestshots,
                        ak47_stomachshots = ak47_stomachshots + @ak47Stomachshots,
                        m4a4_stomachshots = m4a4_stomachshots + @m4a4Stomachshots,
                        m4a1s_stomachshots = m4a1s_stomachshots + @m4a1sStomachshots,
                        aug_stomachshots = aug_stomachshots + @augStomachshots,
                        sg553_stomachshots = sg553_stomachshots + @sg553Stomachshots,
                        galilar_stomachshots = galilar_stomachshots + @galilarStomachshots,
                        famas_stomachshots = famas_stomachshots + @famasStomachshots,
                        awp_stomachshots = awp_stomachshots + @awpStomachshots,
                        scout_stomachshots = scout_stomachshots + @scoutStomachshots,
                        scar20_stomachshots = scar20_stomachshots + @scar20Stomachshots,
                        g3sg1_stomachshots = g3sg1_stomachshots + @g3sg1Stomachshots,
                        ssg08_stomachshots = ssg08_stomachshots + @ssg08Stomachshots,
                        deagle_stomachshots = deagle_stomachshots + @deagleStomachshots,
                        glock_stomachshots = glock_stomachshots + @glockStomachshots,
                        usps_stomachshots = usps_stomachshots + @uspsStomachshots,
                        p250_stomachshots = p250_stomachshots + @p250Stomachshots,
                        p2000_stomachshots = p2000_stomachshots + @p2000Stomachshots,
                        fiveseven_stomachshots = fiveseven_stomachshots + @fivesevenStomachshots,
                        tec9_stomachshots = tec9_stomachshots + @tec9Stomachshots,
                        cz75_stomachshots = cz75_stomachshots + @cz75Stomachshots,
                        dualies_stomachshots = dualies_stomachshots + @dualiesStomachshots,
                        revolver_stomachshots = revolver_stomachshots + @revolverStomachshots,
                        nova_stomachshots = nova_stomachshots + @novaStomachshots,
                        xm1014_stomachshots = xm1014_stomachshots + @xm1014Stomachshots,
                        sawedoff_stomachshots = sawedoff_stomachshots + @sawedoffStomachshots,
                        mag7_stomachshots = mag7_stomachshots + @mag7Stomachshots,
                        mac10_stomachshots = mac10_stomachshots + @mac10Stomachshots,
                        mp9_stomachshots = mp9_stomachshots + @mp9Stomachshots,
                        mp7_stomachshots = mp7_stomachshots + @mp7Stomachshots,
                        ump45_stomachshots = ump45_stomachshots + @ump45Stomachshots,
                        p90_stomachshots = p90_stomachshots + @p90Stomachshots,
                        bizon_stomachshots = bizon_stomachshots + @bizonStomachshots,
                        negev_stomachshots = negev_stomachshots + @negevStomachshots,
                        m249_stomachshots = m249_stomachshots + @m249Stomachshots,
                        ak47_leftlegshots = ak47_leftlegshots + @ak47LeftLegshots,
                        m4a4_leftlegshots = m4a4_leftlegshots + @m4a4LeftLegshots,
                        m4a1s_leftlegshots = m4a1s_leftlegshots + @m4a1sLeftLegshots,
                        aug_leftlegshots = aug_leftlegshots + @augLeftLegshots,
                        sg553_leftlegshots = sg553_leftlegshots + @sg553LeftLegshots,
                        galilar_leftlegshots = galilar_leftlegshots + @galilarLeftLegshots,
                        famas_leftlegshots = famas_leftlegshots + @famasLeftLegshots,
                        awp_leftlegshots = awp_leftlegshots + @awpLeftLegshots,
                        scout_leftlegshots = scout_leftlegshots + @scoutLeftLegshots,
                        scar20_leftlegshots = scar20_leftlegshots + @scar20LeftLegshots,
                        g3sg1_leftlegshots = g3sg1_leftlegshots + @g3sg1LeftLegshots,
                        ssg08_leftlegshots = ssg08_leftlegshots + @ssg08LeftLegshots,
                        deagle_leftlegshots = deagle_leftlegshots + @deagleLeftLegshots,
                        glock_leftlegshots = glock_leftlegshots + @glockLeftLegshots,
                        usps_leftlegshots = usps_leftlegshots + @uspsLeftLegshots,
                        p250_leftlegshots = p250_leftlegshots + @p250LeftLegshots,
                        p2000_leftlegshots = p2000_leftlegshots + @p2000LeftLegshots,
                        fiveseven_leftlegshots = fiveseven_leftlegshots + @fivesevenLeftLegshots,
                        tec9_leftlegshots = tec9_leftlegshots + @tec9LeftLegshots,
                        cz75_leftlegshots = cz75_leftlegshots + @cz75LeftLegshots,
                        dualies_leftlegshots = dualies_leftlegshots + @dualiesLeftLegshots,
                        revolver_leftlegshots = revolver_leftlegshots + @revolverLeftLegshots,
                        nova_leftlegshots = nova_leftlegshots + @novaLeftLegshots,
                        xm1014_leftlegshots = xm1014_leftlegshots + @xm1014LeftLegshots,
                        sawedoff_leftlegshots = sawedoff_leftlegshots + @sawedoffLeftLegshots,
                        mag7_leftlegshots = mag7_leftlegshots + @mag7LeftLegshots,
                        mac10_leftlegshots = mac10_leftlegshots + @mac10LeftLegshots,
                        mp9_leftlegshots = mp9_leftlegshots + @mp9LeftLegshots,
                        mp7_leftlegshots = mp7_leftlegshots + @mp7LeftLegshots,
                        ump45_leftlegshots = ump45_leftlegshots + @ump45LeftLegshots,
                        p90_leftlegshots = p90_leftlegshots + @p90LeftLegshots,
                        bizon_leftlegshots = bizon_leftlegshots + @bizonLeftLegshots,
                        negev_leftlegshots = negev_leftlegshots + @negevLeftLegshots,
                        m249_leftlegshots = m249_leftlegshots + @m249LeftLegshots,
                        ak47_rightlegshots = ak47_rightlegshots + @ak47RightLegshots,
                        m4a4_rightlegshots = m4a4_rightlegshots + @m4a4RightLegshots,
                        m4a1s_rightlegshots = m4a1s_rightlegshots + @m4a1sRightLegshots,
                        aug_rightlegshots = aug_rightlegshots + @augRightLegshots,
                        sg553_rightlegshots = sg553_rightlegshots + @sg553RightLegshots,
                        galilar_rightlegshots = galilar_rightlegshots + @galilarRightLegshots,
                        famas_rightlegshots = famas_rightlegshots + @famasRightLegshots,
                        awp_rightlegshots = awp_rightlegshots + @awpRightLegshots,
                        scout_rightlegshots = scout_rightlegshots + @scoutRightLegshots,
                        scar20_rightlegshots = scar20_rightlegshots + @scar20RightLegshots,
                        g3sg1_rightlegshots = g3sg1_rightlegshots + @g3sg1RightLegshots,
                        ssg08_rightlegshots = ssg08_rightlegshots + @ssg08RightLegshots,
                        deagle_rightlegshots = deagle_rightlegshots + @deagleRightLegshots,
                        glock_rightlegshots = glock_rightlegshots + @glockRightLegshots,
                        usps_rightlegshots = usps_rightlegshots + @uspsRightLegshots,
                        p250_rightlegshots = p250_rightlegshots + @p250RightLegshots,
                        p2000_rightlegshots = p2000_rightlegshots + @p2000RightLegshots,
                        fiveseven_rightlegshots = fiveseven_rightlegshots + @fivesevenRightLegshots,
                        tec9_rightlegshots = tec9_rightlegshots + @tec9RightLegshots,
                        cz75_rightlegshots = cz75_rightlegshots + @cz75RightLegshots,
                        dualies_rightlegshots = dualies_rightlegshots + @dualiesRightLegshots,
                        revolver_rightlegshots = revolver_rightlegshots + @revolverRightLegshots,
                        nova_rightlegshots = nova_rightlegshots + @novaRightLegshots,
                        xm1014_rightlegshots = xm1014_rightlegshots + @xm1014RightLegshots,
                        sawedoff_rightlegshots = sawedoff_rightlegshots + @sawedoffRightLegshots,
                        mag7_rightlegshots = mag7_rightlegshots + @mag7RightLegshots,
                        mac10_rightlegshots = mac10_rightlegshots + @mac10RightLegshots,
                        mp9_rightlegshots = mp9_rightlegshots + @mp9RightLegshots,
                        mp7_rightlegshots = mp7_rightlegshots + @mp7RightLegshots,
                        ump45_rightlegshots = ump45_rightlegshots + @ump45RightLegshots,
                        p90_rightlegshots = p90_rightlegshots + @p90RightLegshots,
                        bizon_rightlegshots = bizon_rightlegshots + @bizonRightLegshots,
                        negev_rightlegshots = negev_rightlegshots + @negevRightLegshots,
                        m249_rightlegshots = m249_rightlegshots + @m249RightLegshots,
                        last_updated = CURRENT_TIMESTAMP";

                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@steamId", stats.SteamId);
                command.Parameters.AddWithValue("@playerName", stats.PlayerName);
                command.Parameters.AddWithValue("@totalKills", stats.TotalKills);
                command.Parameters.AddWithValue("@totalDeaths", stats.TotalDeaths);
                command.Parameters.AddWithValue("@totalAssists", stats.TotalAssists);
                command.Parameters.AddWithValue("@totalHeadshots", stats.TotalHeadshots);
                command.Parameters.AddWithValue("@totalRoundsPlayed", stats.TotalRoundsPlayed);
                command.Parameters.AddWithValue("@totalDamageDone", stats.TotalDamageDone);
                command.Parameters.AddWithValue("@totalDamageTaken", stats.TotalDamageTaken);
                command.Parameters.AddWithValue("@ak47Kills", stats.AK47Kills);
                command.Parameters.AddWithValue("@m4a4Kills", stats.M4A4Kills);
                command.Parameters.AddWithValue("@m4a1sKills", stats.M4A1SKills);
                command.Parameters.AddWithValue("@augKills", stats.AUGKills);
                command.Parameters.AddWithValue("@sg553Kills", stats.SG553Kills);
                command.Parameters.AddWithValue("@galilarKills", stats.GalilARKills);
                command.Parameters.AddWithValue("@famasKills", stats.FAMASKills);
                command.Parameters.AddWithValue("@awpKills", stats.AWPKills);
                command.Parameters.AddWithValue("@scoutKills", stats.ScoutKills);
                command.Parameters.AddWithValue("@scar20Kills", stats.Scar20Kills);
                command.Parameters.AddWithValue("@g3sg1Kills", stats.G3SG1Kills);
                command.Parameters.AddWithValue("@ssg08Kills", stats.SSG08Kills);
                command.Parameters.AddWithValue("@deagleKills", stats.DeagleKills);
                command.Parameters.AddWithValue("@glockKills", stats.GlockKills);
                command.Parameters.AddWithValue("@uspsKills", stats.USPSKills);
                command.Parameters.AddWithValue("@p250Kills", stats.P250Kills);
                command.Parameters.AddWithValue("@p2000Kills", stats.P2000Kills);
                command.Parameters.AddWithValue("@fivesevenKills", stats.FiveSevenKills);
                command.Parameters.AddWithValue("@tec9Kills", stats.Tec9Kills);
                command.Parameters.AddWithValue("@cz75Kills", stats.CZ75Kills);
                command.Parameters.AddWithValue("@dualiesKills", stats.DualiesKills);
                command.Parameters.AddWithValue("@revolverKills", stats.RevolverKills);
                command.Parameters.AddWithValue("@novaKills", stats.NovaKills);
                command.Parameters.AddWithValue("@xm1014Kills", stats.XM1014Kills);
                command.Parameters.AddWithValue("@sawedoffKills", stats.SawedOffKills);
                command.Parameters.AddWithValue("@mag7Kills", stats.MAG7Kills);
                command.Parameters.AddWithValue("@mac10Kills", stats.Mac10Kills);
                command.Parameters.AddWithValue("@mp9Kills", stats.MP9Kills);
                command.Parameters.AddWithValue("@mp7Kills", stats.MP7Kills);
                command.Parameters.AddWithValue("@ump45Kills", stats.UMP45Kills);
                command.Parameters.AddWithValue("@p90Kills", stats.P90Kills);
                command.Parameters.AddWithValue("@bizonKills", stats.BizonKills);
                command.Parameters.AddWithValue("@negevKills", stats.NegevKills);
                command.Parameters.AddWithValue("@m249Kills", stats.M249Kills);
                command.Parameters.AddWithValue("@ak47Deaths", stats.AK47Deaths);
                command.Parameters.AddWithValue("@m4a4Deaths", stats.M4A4Deaths);
                command.Parameters.AddWithValue("@m4a1sDeaths", stats.M4A1SDeaths);
                command.Parameters.AddWithValue("@augDeaths", stats.AUGDeaths);
                command.Parameters.AddWithValue("@sg553Deaths", stats.SG553Deaths);
                command.Parameters.AddWithValue("@galilarDeaths", stats.GalilARDeaths);
                command.Parameters.AddWithValue("@famasDeaths", stats.FAMASDeaths);
                command.Parameters.AddWithValue("@awpDeaths", stats.AWPDeaths);
                command.Parameters.AddWithValue("@scoutDeaths", stats.ScoutDeaths);
                command.Parameters.AddWithValue("@scar20Deaths", stats.Scar20Deaths);
                command.Parameters.AddWithValue("@g3sg1Deaths", stats.G3SG1Deaths);
                command.Parameters.AddWithValue("@ssg08Deaths", stats.SSG08Deaths);
                command.Parameters.AddWithValue("@deagleDeaths", stats.DeagleDeaths);
                command.Parameters.AddWithValue("@glockDeaths", stats.GlockDeaths);
                command.Parameters.AddWithValue("@uspsDeaths", stats.USPSDeaths);
                command.Parameters.AddWithValue("@p250Deaths", stats.P250Deaths);
                command.Parameters.AddWithValue("@p2000Deaths", stats.P2000Deaths);
                command.Parameters.AddWithValue("@fivesevenDeaths", stats.FiveSevenDeaths);
                command.Parameters.AddWithValue("@tec9Deaths", stats.Tec9Deaths);
                command.Parameters.AddWithValue("@cz75Deaths", stats.CZ75Deaths);
                command.Parameters.AddWithValue("@dualiesDeaths", stats.DualiesDeaths);
                command.Parameters.AddWithValue("@revolverDeaths", stats.RevolverDeaths);
                command.Parameters.AddWithValue("@novaDeaths", stats.NovaDeaths);
                command.Parameters.AddWithValue("@xm1014Deaths", stats.XM1014Deaths);
                command.Parameters.AddWithValue("@sawedoffDeaths", stats.SawedOffDeaths);
                command.Parameters.AddWithValue("@mag7Deaths", stats.MAG7Deaths);
                command.Parameters.AddWithValue("@mac10Deaths", stats.Mac10Deaths);
                command.Parameters.AddWithValue("@mp9Deaths", stats.MP9Deaths);
                command.Parameters.AddWithValue("@mp7Deaths", stats.MP7Deaths);
                command.Parameters.AddWithValue("@ump45Deaths", stats.UMP45Deaths);
                command.Parameters.AddWithValue("@p90Deaths", stats.P90Deaths);
                command.Parameters.AddWithValue("@bizonDeaths", stats.BizonDeaths);
                command.Parameters.AddWithValue("@negevDeaths", stats.NegevDeaths);
                command.Parameters.AddWithValue("@m249Deaths", stats.M249Deaths);
                command.Parameters.AddWithValue("@ak47Assists", stats.AK47Assists);
                command.Parameters.AddWithValue("@m4a4Assists", stats.M4A4Assists);
                command.Parameters.AddWithValue("@m4a1sAssists", stats.M4A1SAssists);
                command.Parameters.AddWithValue("@augAssists", stats.AUGAssists);
                command.Parameters.AddWithValue("@sg553Assists", stats.SG553Assists);
                command.Parameters.AddWithValue("@galilarAssists", stats.GalilARAssists);
                command.Parameters.AddWithValue("@famasAssists", stats.FAMASAssists);
                command.Parameters.AddWithValue("@awpAssists", stats.AWPAssists);
                command.Parameters.AddWithValue("@scoutAssists", stats.ScoutAssists);
                command.Parameters.AddWithValue("@scar20Assists", stats.Scar20Assists);
                command.Parameters.AddWithValue("@g3sg1Assists", stats.G3SG1Assists);
                command.Parameters.AddWithValue("@ssg08Assists", stats.SSG08Assists);
                command.Parameters.AddWithValue("@deagleAssists", stats.DeagleAssists);
                command.Parameters.AddWithValue("@glockAssists", stats.GlockAssists);
                command.Parameters.AddWithValue("@uspsAssists", stats.USPSAssists);
                command.Parameters.AddWithValue("@p250Assists", stats.P250Assists);
                command.Parameters.AddWithValue("@p2000Assists", stats.P2000Assists);
                command.Parameters.AddWithValue("@fivesevenAssists", stats.FiveSevenAssists);
                command.Parameters.AddWithValue("@tec9Assists", stats.Tec9Assists);
                command.Parameters.AddWithValue("@cz75Assists", stats.CZ75Assists);
                command.Parameters.AddWithValue("@dualiesAssists", stats.DualiesAssists);
                command.Parameters.AddWithValue("@revolverAssists", stats.RevolverAssists);
                command.Parameters.AddWithValue("@novaAssists", stats.NovaAssists);
                command.Parameters.AddWithValue("@xm1014Assists", stats.XM1014Assists);
                command.Parameters.AddWithValue("@sawedoffAssists", stats.SawedOffAssists);
                command.Parameters.AddWithValue("@mag7Assists", stats.MAG7Assists);
                command.Parameters.AddWithValue("@mac10Assists", stats.Mac10Assists);
                command.Parameters.AddWithValue("@mp9Assists", stats.MP9Assists);
                command.Parameters.AddWithValue("@mp7Assists", stats.MP7Assists);
                command.Parameters.AddWithValue("@ump45Assists", stats.UMP45Assists);
                command.Parameters.AddWithValue("@p90Assists", stats.P90Assists);
                command.Parameters.AddWithValue("@bizonAssists", stats.BizonAssists);
                command.Parameters.AddWithValue("@negevAssists", stats.NegevAssists);
                command.Parameters.AddWithValue("@m249Assists", stats.M249Assists);
                command.Parameters.AddWithValue("@ak47Headshots", stats.AK47Headshots);
                command.Parameters.AddWithValue("@m4a4Headshots", stats.M4A4Headshots);
                command.Parameters.AddWithValue("@m4a1sHeadshots", stats.M4A1SHeadshots);
                command.Parameters.AddWithValue("@augHeadshots", stats.AUGHeadshots);
                command.Parameters.AddWithValue("@sg553Headshots", stats.SG553Headshots);
                command.Parameters.AddWithValue("@galilarHeadshots", stats.GalilARHeadshots);
                command.Parameters.AddWithValue("@famasHeadshots", stats.FAMASHeadshots);
                command.Parameters.AddWithValue("@awpHeadshots", stats.AWPHeadshots);
                command.Parameters.AddWithValue("@scoutHeadshots", stats.ScoutHeadshots);
                command.Parameters.AddWithValue("@scar20Headshots", stats.Scar20Headshots);
                command.Parameters.AddWithValue("@g3sg1Headshots", stats.G3SG1Headshots);
                command.Parameters.AddWithValue("@ssg08Headshots", stats.SSG08Headshots);
                command.Parameters.AddWithValue("@deagleHeadshots", stats.DeagleHeadshots);
                command.Parameters.AddWithValue("@glockHeadshots", stats.GlockHeadshots);
                command.Parameters.AddWithValue("@uspsHeadshots", stats.USPSHeadshots);
                command.Parameters.AddWithValue("@p250Headshots", stats.P250Headshots);
                command.Parameters.AddWithValue("@p2000Headshots", stats.P2000Headshots);
                command.Parameters.AddWithValue("@fivesevenHeadshots", stats.FiveSevenHeadshots);
                command.Parameters.AddWithValue("@tec9Headshots", stats.Tec9Headshots);
                command.Parameters.AddWithValue("@cz75Headshots", stats.CZ75Headshots);
                command.Parameters.AddWithValue("@dualiesHeadshots", stats.DualiesHeadshots);
                command.Parameters.AddWithValue("@revolverHeadshots", stats.RevolverHeadshots);
                command.Parameters.AddWithValue("@novaHeadshots", stats.NovaHeadshots);
                command.Parameters.AddWithValue("@xm1014Headshots", stats.XM1014Headshots);
                command.Parameters.AddWithValue("@sawedoffHeadshots", stats.SawedOffHeadshots);
                command.Parameters.AddWithValue("@mag7Headshots", stats.MAG7Headshots);
                command.Parameters.AddWithValue("@mac10Headshots", stats.Mac10Headshots);
                command.Parameters.AddWithValue("@mp9Headshots", stats.MP9Headshots);
                command.Parameters.AddWithValue("@mp7Headshots", stats.MP7Headshots);
                command.Parameters.AddWithValue("@ump45Headshots", stats.UMP45Headshots);
                command.Parameters.AddWithValue("@p90Headshots", stats.P90Headshots);
                command.Parameters.AddWithValue("@bizonHeadshots", stats.BizonHeadshots);
                command.Parameters.AddWithValue("@negevHeadshots", stats.NegevHeadshots);
                command.Parameters.AddWithValue("@m249Headshots", stats.M249Headshots);
                command.Parameters.AddWithValue("@ak47Chestshots", stats.AK47Chestshots);
                command.Parameters.AddWithValue("@m4a4Chestshots", stats.M4A4Chestshots);
                command.Parameters.AddWithValue("@m4a1sChestshots", stats.M4A1SChestshots);
                command.Parameters.AddWithValue("@augChestshots", stats.AUGChestshots);
                command.Parameters.AddWithValue("@sg553Chestshots", stats.SG553Chestshots);
                command.Parameters.AddWithValue("@galilarChestshots", stats.GalilARChestshots);
                command.Parameters.AddWithValue("@famasChestshots", stats.FAMASChestshots);
                command.Parameters.AddWithValue("@awpChestshots", stats.AWPChestshots);
                command.Parameters.AddWithValue("@scoutChestshots", stats.ScoutChestshots);
                command.Parameters.AddWithValue("@scar20Chestshots", stats.Scar20Chestshots);
                command.Parameters.AddWithValue("@g3sg1Chestshots", stats.G3SG1Chestshots);
                command.Parameters.AddWithValue("@ssg08Chestshots", stats.SSG08Chestshots);
                command.Parameters.AddWithValue("@deagleChestshots", stats.DeagleChestshots);
                command.Parameters.AddWithValue("@glockChestshots", stats.GlockChestshots);
                command.Parameters.AddWithValue("@uspsChestshots", stats.USPSChestshots);
                command.Parameters.AddWithValue("@p250Chestshots", stats.P250Chestshots);
                command.Parameters.AddWithValue("@p2000Chestshots", stats.P2000Chestshots);
                command.Parameters.AddWithValue("@fivesevenChestshots", stats.FiveSevenChestshots);
                command.Parameters.AddWithValue("@tec9Chestshots", stats.Tec9Chestshots);
                command.Parameters.AddWithValue("@cz75Chestshots", stats.CZ75Chestshots);
                command.Parameters.AddWithValue("@dualiesChestshots", stats.DualiesChestshots);
                command.Parameters.AddWithValue("@revolverChestshots", stats.RevolverChestshots);
                command.Parameters.AddWithValue("@novaChestshots", stats.NovaChestshots);
                command.Parameters.AddWithValue("@xm1014Chestshots", stats.XM1014Chestshots);
                command.Parameters.AddWithValue("@sawedoffChestshots", stats.SawedOffChestshots);
                command.Parameters.AddWithValue("@mag7Chestshots", stats.MAG7Chestshots);
                command.Parameters.AddWithValue("@mac10Chestshots", stats.Mac10Chestshots);
                command.Parameters.AddWithValue("@mp9Chestshots", stats.MP9Chestshots);
                command.Parameters.AddWithValue("@mp7Chestshots", stats.MP7Chestshots);
                command.Parameters.AddWithValue("@ump45Chestshots", stats.UMP45Chestshots);
                command.Parameters.AddWithValue("@p90Chestshots", stats.P90Chestshots);
                command.Parameters.AddWithValue("@bizonChestshots", stats.BizonChestshots);
                command.Parameters.AddWithValue("@negevChestshots", stats.NegevChestshots);
                command.Parameters.AddWithValue("@m249Chestshots", stats.M249Chestshots);
                command.Parameters.AddWithValue("@ak47Stomachshots", stats.AK47Stomachshots);
                command.Parameters.AddWithValue("@m4a4Stomachshots", stats.M4A4Stomachshots);
                command.Parameters.AddWithValue("@m4a1sStomachshots", stats.M4A1SStomachshots);
                command.Parameters.AddWithValue("@augStomachshots", stats.AUGStomachshots);
                command.Parameters.AddWithValue("@sg553Stomachshots", stats.SG553Stomachshots);
                command.Parameters.AddWithValue("@galilarStomachshots", stats.GalilARStomachshots);
                command.Parameters.AddWithValue("@famasStomachshots", stats.FAMASStomachshots);
                command.Parameters.AddWithValue("@awpStomachshots", stats.AWPStomachshots);
                command.Parameters.AddWithValue("@scoutStomachshots", stats.ScoutStomachshots);
                command.Parameters.AddWithValue("@scar20Stomachshots", stats.Scar20Stomachshots);
                command.Parameters.AddWithValue("@g3sg1Stomachshots", stats.G3SG1Stomachshots);
                command.Parameters.AddWithValue("@ssg08Stomachshots", stats.SSG08Stomachshots);
                command.Parameters.AddWithValue("@deagleStomachshots", stats.DeagleStomachshots);
                command.Parameters.AddWithValue("@glockStomachshots", stats.GlockStomachshots);
                command.Parameters.AddWithValue("@uspsStomachshots", stats.USPSStomachshots);
                command.Parameters.AddWithValue("@p250Stomachshots", stats.P250Stomachshots);
                command.Parameters.AddWithValue("@p2000Stomachshots", stats.P2000Stomachshots);
                command.Parameters.AddWithValue("@fivesevenStomachshots", stats.FiveSevenStomachshots);
                command.Parameters.AddWithValue("@tec9Stomachshots", stats.Tec9Stomachshots);
                command.Parameters.AddWithValue("@cz75Stomachshots", stats.CZ75Stomachshots);
                command.Parameters.AddWithValue("@dualiesStomachshots", stats.DualiesStomachshots);
                command.Parameters.AddWithValue("@revolverStomachshots", stats.RevolverStomachshots);
                command.Parameters.AddWithValue("@novaStomachshots", stats.NovaStomachshots);
                command.Parameters.AddWithValue("@xm1014Stomachshots", stats.XM1014Stomachshots);
                command.Parameters.AddWithValue("@sawedoffStomachshots", stats.SawedOffStomachshots);
                command.Parameters.AddWithValue("@mag7Stomachshots", stats.MAG7Stomachshots);
                command.Parameters.AddWithValue("@mac10Stomachshots", stats.Mac10Stomachshots);
                command.Parameters.AddWithValue("@mp9Stomachshots", stats.MP9Stomachshots);
                command.Parameters.AddWithValue("@mp7Stomachshots", stats.MP7Stomachshots);
                command.Parameters.AddWithValue("@ump45Stomachshots", stats.UMP45Stomachshots);
                command.Parameters.AddWithValue("@p90Stomachshots", stats.P90Stomachshots);
                command.Parameters.AddWithValue("@bizonStomachshots", stats.BizonStomachshots);
                command.Parameters.AddWithValue("@negevStomachshots", stats.NegevStomachshots);
                command.Parameters.AddWithValue("@m249Stomachshots", stats.M249Stomachshots);
                command.Parameters.AddWithValue("@ak47LeftLegshots", stats.AK47LeftLegshots);
                command.Parameters.AddWithValue("@m4a4LeftLegshots", stats.M4A4LeftLegshots);
                command.Parameters.AddWithValue("@m4a1sLeftLegshots", stats.M4A1SLeftLegshots);
                command.Parameters.AddWithValue("@augLeftLegshots", stats.AUGLeftLegshots);
                command.Parameters.AddWithValue("@sg553LeftLegshots", stats.SG553LeftLegshots);
                command.Parameters.AddWithValue("@galilarLeftLegshots", stats.GalilARLeftLegshots);
                command.Parameters.AddWithValue("@famasLeftLegshots", stats.FAMASLeftLegshots);
                command.Parameters.AddWithValue("@awpLeftLegshots", stats.AWPLeftLegshots);
                command.Parameters.AddWithValue("@scoutLeftLegshots", stats.ScoutLeftLegshots);
                command.Parameters.AddWithValue("@scar20LeftLegshots", stats.Scar20LeftLegshots);
                command.Parameters.AddWithValue("@g3sg1LeftLegshots", stats.G3SG1LeftLegshots);
                command.Parameters.AddWithValue("@ssg08LeftLegshots", stats.SSG08LeftLegshots);
                command.Parameters.AddWithValue("@deagleLeftLegshots", stats.DeagleLeftLegshots);
                command.Parameters.AddWithValue("@glockLeftLegshots", stats.GlockLeftLegshots);
                command.Parameters.AddWithValue("@uspsLeftLegshots", stats.USPSLeftLegshots);
                command.Parameters.AddWithValue("@p250LeftLegshots", stats.P250LeftLegshots);
                command.Parameters.AddWithValue("@p2000LeftLegshots", stats.P2000LeftLegshots);
                command.Parameters.AddWithValue("@fivesevenLeftLegshots", stats.FiveSevenLeftLegshots);
                command.Parameters.AddWithValue("@tec9LeftLegshots", stats.Tec9LeftLegshots);
                command.Parameters.AddWithValue("@cz75LeftLegshots", stats.CZ75LeftLegshots);
                command.Parameters.AddWithValue("@dualiesLeftLegshots", stats.DualiesLeftLegshots);
                command.Parameters.AddWithValue("@revolverLeftLegshots", stats.RevolverLeftLegshots);
                command.Parameters.AddWithValue("@novaLeftLegshots", stats.NovaLeftLegshots);
                command.Parameters.AddWithValue("@xm1014LeftLegshots", stats.XM1014LeftLegshots);
                command.Parameters.AddWithValue("@sawedoffLeftLegshots", stats.SawedOffLeftLegshots);
                command.Parameters.AddWithValue("@mag7LeftLegshots", stats.MAG7LeftLegshots);
                command.Parameters.AddWithValue("@mac10LeftLegshots", stats.Mac10LeftLegshots);
                command.Parameters.AddWithValue("@mp9LeftLegshots", stats.MP9LeftLegshots);
                command.Parameters.AddWithValue("@mp7LeftLegshots", stats.MP7LeftLegshots);
                command.Parameters.AddWithValue("@ump45LeftLegshots", stats.UMP45LeftLegshots);
                command.Parameters.AddWithValue("@p90LeftLegshots", stats.P90LeftLegshots);
                command.Parameters.AddWithValue("@bizonLeftLegshots", stats.BizonLeftLegshots);
                command.Parameters.AddWithValue("@negevLeftLegshots", stats.NegevLeftLegshots);
                command.Parameters.AddWithValue("@m249LeftLegshots", stats.M249LeftLegshots);
                command.Parameters.AddWithValue("@ak47RightLegshots", stats.AK47RightLegshots);
                command.Parameters.AddWithValue("@m4a4RightLegshots", stats.M4A4RightLegshots);
                command.Parameters.AddWithValue("@m4a1sRightLegshots", stats.M4A1SRightLegshots);
                command.Parameters.AddWithValue("@augRightLegshots", stats.AUGRightLegshots);
                command.Parameters.AddWithValue("@sg553RightLegshots", stats.SG553RightLegshots);
                command.Parameters.AddWithValue("@galilarRightLegshots", stats.GalilARRightLegshots);
                command.Parameters.AddWithValue("@famasRightLegshots", stats.FAMASRightLegshots);
                command.Parameters.AddWithValue("@awpRightLegshots", stats.AWPRightLegshots);
                command.Parameters.AddWithValue("@scoutRightLegshots", stats.ScoutRightLegshots);
                command.Parameters.AddWithValue("@scar20RightLegshots", stats.Scar20RightLegshots);
                command.Parameters.AddWithValue("@g3sg1RightLegshots", stats.G3SG1RightLegshots);
                command.Parameters.AddWithValue("@ssg08RightLegshots", stats.SSG08RightLegshots);
                command.Parameters.AddWithValue("@deagleRightLegshots", stats.DeagleRightLegshots);
                command.Parameters.AddWithValue("@glockRightLegshots", stats.GlockRightLegshots);
                command.Parameters.AddWithValue("@uspsRightLegshots", stats.USPSRightLegshots);
                command.Parameters.AddWithValue("@p250RightLegshots", stats.P250RightLegshots);
                command.Parameters.AddWithValue("@p2000RightLegshots", stats.P2000RightLegshots);
                command.Parameters.AddWithValue("@fivesevenRightLegshots", stats.FiveSevenRightLegshots);
                command.Parameters.AddWithValue("@tec9RightLegshots", stats.Tec9RightLegshots);
                command.Parameters.AddWithValue("@cz75RightLegshots", stats.CZ75RightLegshots);
                command.Parameters.AddWithValue("@dualiesRightLegshots", stats.DualiesRightLegshots);
                command.Parameters.AddWithValue("@revolverRightLegshots", stats.RevolverRightLegshots);
                command.Parameters.AddWithValue("@novaRightLegshots", stats.NovaRightLegshots);
                command.Parameters.AddWithValue("@xm1014RightLegshots", stats.XM1014RightLegshots);
                command.Parameters.AddWithValue("@sawedoffRightLegshots", stats.SawedOffRightLegshots);
                command.Parameters.AddWithValue("@mag7RightLegshots", stats.MAG7RightLegshots);
                command.Parameters.AddWithValue("@mac10RightLegshots", stats.Mac10RightLegshots);
                command.Parameters.AddWithValue("@mp9RightLegshots", stats.MP9RightLegshots);
                command.Parameters.AddWithValue("@mp7RightLegshots", stats.MP7RightLegshots);
                command.Parameters.AddWithValue("@ump45RightLegshots", stats.UMP45RightLegshots);
                command.Parameters.AddWithValue("@p90RightLegshots", stats.P90RightLegshots);
                command.Parameters.AddWithValue("@bizonRightLegshots", stats.BizonRightLegshots);
                command.Parameters.AddWithValue("@negevRightLegshots", stats.NegevRightLegshots);
                command.Parameters.AddWithValue("@m249RightLegshots", stats.M249RightLegshots);
                
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to update player stats: {ex.Message}");
                return false;
            }
        }
    }
}