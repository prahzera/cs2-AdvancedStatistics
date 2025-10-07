using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedStatistics.Models
{
    public class PlayerStats
    {
        public int Id { get; set; }
        public string SteamId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        
        // Estadísticas generales
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalAssists { get; set; }
        public int TotalHeadshots { get; set; }
        public int TotalRoundsPlayed { get; set; }
        public int TotalDamageDone { get; set; }
        public int TotalDamageTaken { get; set; }
        
        // Kills por arma
        public int AK47Kills { get; set; }
        public int M4A4Kills { get; set; }
        public int M4A1SKills { get; set; }
        public int AUGKills { get; set; }
        public int SG553Kills { get; set; }
        public int GalilARKills { get; set; }
        public int FAMASKills { get; set; }
        public int AWPKills { get; set; }
        public int ScoutKills { get; set; }
        public int Scar20Kills { get; set; }
        public int G3SG1Kills { get; set; }
        public int SSG08Kills { get; set; }
        public int DeagleKills { get; set; }
        public int GlockKills { get; set; }
        public int USPSKills { get; set; }
        public int P250Kills { get; set; }
        public int P2000Kills { get; set; }
        public int FiveSevenKills { get; set; }
        public int Tec9Kills { get; set; }
        public int CZ75Kills { get; set; }
        public int DualiesKills { get; set; }
        public int RevolverKills { get; set; }
        public int NovaKills { get; set; }
        public int XM1014Kills { get; set; }
        public int SawedOffKills { get; set; }
        public int MAG7Kills { get; set; }
        public int Mac10Kills { get; set; }
        public int MP9Kills { get; set; }
        public int MP7Kills { get; set; }
        public int UMP45Kills { get; set; }
        public int P90Kills { get; set; }
        public int BizonKills { get; set; }
        public int NegevKills { get; set; }
        public int M249Kills { get; set; }
        
        // Muertes por arma
        public int AK47Deaths { get; set; }
        public int M4A4Deaths { get; set; }
        public int M4A1SDeaths { get; set; }
        public int AUGDeaths { get; set; }
        public int SG553Deaths { get; set; }
        public int GalilARDeaths { get; set; }
        public int FAMASDeaths { get; set; }
        public int AWPDeaths { get; set; }
        public int ScoutDeaths { get; set; }
        public int Scar20Deaths { get; set; }
        public int G3SG1Deaths { get; set; }
        public int SSG08Deaths { get; set; }
        public int DeagleDeaths { get; set; }
        public int GlockDeaths { get; set; }
        public int USPSDeaths { get; set; }
        public int P250Deaths { get; set; }
        public int P2000Deaths { get; set; }
        public int FiveSevenDeaths { get; set; }
        public int Tec9Deaths { get; set; }
        public int CZ75Deaths { get; set; }
        public int DualiesDeaths { get; set; }
        public int RevolverDeaths { get; set; }
        public int NovaDeaths { get; set; }
        public int XM1014Deaths { get; set; }
        public int SawedOffDeaths { get; set; }
        public int MAG7Deaths { get; set; }
        public int Mac10Deaths { get; set; }
        public int MP9Deaths { get; set; }
        public int MP7Deaths { get; set; }
        public int UMP45Deaths { get; set; }
        public int P90Deaths { get; set; }
        public int BizonDeaths { get; set; }
        public int NegevDeaths { get; set; }
        public int M249Deaths { get; set; }
        
        // Asistencias por arma
        public int AK47Assists { get; set; }
        public int M4A4Assists { get; set; }
        public int M4A1SAssists { get; set; }
        public int AUGAssists { get; set; }
        public int SG553Assists { get; set; }
        public int GalilARAssists { get; set; }
        public int FAMASAssists { get; set; }
        public int AWPAssists { get; set; }
        public int ScoutAssists { get; set; }
        public int Scar20Assists { get; set; }
        public int G3SG1Assists { get; set; }
        public int SSG08Assists { get; set; }
        public int DeagleAssists { get; set; }
        public int GlockAssists { get; set; }
        public int USPSAssists { get; set; }
        public int P250Assists { get; set; }
        public int P2000Assists { get; set; }
        public int FiveSevenAssists { get; set; }
        public int Tec9Assists { get; set; }
        public int CZ75Assists { get; set; }
        public int DualiesAssists { get; set; }
        public int RevolverAssists { get; set; }
        public int NovaAssists { get; set; }
        public int XM1014Assists { get; set; }
        public int SawedOffAssists { get; set; }
        public int MAG7Assists { get; set; }
        public int Mac10Assists { get; set; }
        public int MP9Assists { get; set; }
        public int MP7Assists { get; set; }
        public int UMP45Assists { get; set; }
        public int P90Assists { get; set; }
        public int BizonAssists { get; set; }
        public int NegevAssists { get; set; }
        public int M249Assists { get; set; }
        
        // Headshots por arma
        public int AK47Headshots { get; set; }
        public int M4A4Headshots { get; set; }
        public int M4A1SHeadshots { get; set; }
        public int AUGHeadshots { get; set; }
        public int SG553Headshots { get; set; }
        public int GalilARHeadshots { get; set; }
        public int FAMASHeadshots { get; set; }
        public int AWPHeadshots { get; set; }
        public int ScoutHeadshots { get; set; }
        public int Scar20Headshots { get; set; }
        public int G3SG1Headshots { get; set; }
        public int SSG08Headshots { get; set; }
        public int DeagleHeadshots { get; set; }
        public int GlockHeadshots { get; set; }
        public int USPSHeadshots { get; set; }
        public int P250Headshots { get; set; }
        public int P2000Headshots { get; set; }
        public int FiveSevenHeadshots { get; set; }
        public int Tec9Headshots { get; set; }
        public int CZ75Headshots { get; set; }
        public int DualiesHeadshots { get; set; }
        public int RevolverHeadshots { get; set; }
        public int NovaHeadshots { get; set; }
        public int XM1014Headshots { get; set; }
        public int SawedOffHeadshots { get; set; }
        public int MAG7Headshots { get; set; }
        public int Mac10Headshots { get; set; }
        public int MP9Headshots { get; set; }
        public int MP7Headshots { get; set; }
        public int UMP45Headshots { get; set; }
        public int P90Headshots { get; set; }
        public int BizonHeadshots { get; set; }
        public int NegevHeadshots { get; set; }
        public int M249Headshots { get; set; }
        
        // Chest shots por arma
        public int AK47Chestshots { get; set; }
        public int M4A4Chestshots { get; set; }
        public int M4A1SChestshots { get; set; }
        public int AUGChestshots { get; set; }
        public int SG553Chestshots { get; set; }
        public int GalilARChestshots { get; set; }
        public int FAMASChestshots { get; set; }
        public int AWPChestshots { get; set; }
        public int ScoutChestshots { get; set; }
        public int Scar20Chestshots { get; set; }
        public int G3SG1Chestshots { get; set; }
        public int SSG08Chestshots { get; set; }
        public int DeagleChestshots { get; set; }
        public int GlockChestshots { get; set; }
        public int USPSChestshots { get; set; }
        public int P250Chestshots { get; set; }
        public int P2000Chestshots { get; set; }
        public int FiveSevenChestshots { get; set; }
        public int Tec9Chestshots { get; set; }
        public int CZ75Chestshots { get; set; }
        public int DualiesChestshots { get; set; }
        public int RevolverChestshots { get; set; }
        public int NovaChestshots { get; set; }
        public int XM1014Chestshots { get; set; }
        public int SawedOffChestshots { get; set; }
        public int MAG7Chestshots { get; set; }
        public int Mac10Chestshots { get; set; }
        public int MP9Chestshots { get; set; }
        public int MP7Chestshots { get; set; }
        public int UMP45Chestshots { get; set; }
        public int P90Chestshots { get; set; }
        public int BizonChestshots { get; set; }
        public int NegevChestshots { get; set; }
        public int M249Chestshots { get; set; }
        
        // Stomach shots por arma
        public int AK47Stomachshots { get; set; }
        public int M4A4Stomachshots { get; set; }
        public int M4A1SStomachshots { get; set; }
        public int AUGStomachshots { get; set; }
        public int SG553Stomachshots { get; set; }
        public int GalilARStomachshots { get; set; }
        public int FAMASStomachshots { get; set; }
        public int AWPStomachshots { get; set; }
        public int ScoutStomachshots { get; set; }
        public int Scar20Stomachshots { get; set; }
        public int G3SG1Stomachshots { get; set; }
        public int SSG08Stomachshots { get; set; }
        public int DeagleStomachshots { get; set; }
        public int GlockStomachshots { get; set; }
        public int USPSStomachshots { get; set; }
        public int P250Stomachshots { get; set; }
        public int P2000Stomachshots { get; set; }
        public int FiveSevenStomachshots { get; set; }
        public int Tec9Stomachshots { get; set; }
        public int CZ75Stomachshots { get; set; }
        public int DualiesStomachshots { get; set; }
        public int RevolverStomachshots { get; set; }
        public int NovaStomachshots { get; set; }
        public int XM1014Stomachshots { get; set; }
        public int SawedOffStomachshots { get; set; }
        public int MAG7Stomachshots { get; set; }
        public int Mac10Stomachshots { get; set; }
        public int MP9Stomachshots { get; set; }
        public int MP7Stomachshots { get; set; }
        public int UMP45Stomachshots { get; set; }
        public int P90Stomachshots { get; set; }
        public int BizonStomachshots { get; set; }
        public int NegevStomachshots { get; set; }
        public int M249Stomachshots { get; set; }
        
        // Left leg shots por arma
        public int AK47LeftLegshots { get; set; }
        public int M4A4LeftLegshots { get; set; }
        public int M4A1SLeftLegshots { get; set; }
        public int AUGLeftLegshots { get; set; }
        public int SG553LeftLegshots { get; set; }
        public int GalilARLeftLegshots { get; set; }
        public int FAMASLeftLegshots { get; set; }
        public int AWPLeftLegshots { get; set; }
        public int ScoutLeftLegshots { get; set; }
        public int Scar20LeftLegshots { get; set; }
        public int G3SG1LeftLegshots { get; set; }
        public int SSG08LeftLegshots { get; set; }
        public int DeagleLeftLegshots { get; set; }
        public int GlockLeftLegshots { get; set; }
        public int USPSLeftLegshots { get; set; }
        public int P250LeftLegshots { get; set; }
        public int P2000LeftLegshots { get; set; }
        public int FiveSevenLeftLegshots { get; set; }
        public int Tec9LeftLegshots { get; set; }
        public int CZ75LeftLegshots { get; set; }
        public int DualiesLeftLegshots { get; set; }
        public int RevolverLeftLegshots { get; set; }
        public int NovaLeftLegshots { get; set; }
        public int XM1014LeftLegshots { get; set; }
        public int SawedOffLeftLegshots { get; set; }
        public int MAG7LeftLegshots { get; set; }
        public int Mac10LeftLegshots { get; set; }
        public int MP9LeftLegshots { get; set; }
        public int MP7LeftLegshots { get; set; }
        public int UMP45LeftLegshots { get; set; }
        public int P90LeftLegshots { get; set; }
        public int BizonLeftLegshots { get; set; }
        public int NegevLeftLegshots { get; set; }
        public int M249LeftLegshots { get; set; }
        
        // Right leg shots por arma
        public int AK47RightLegshots { get; set; }
        public int M4A4RightLegshots { get; set; }
        public int M4A1SRightLegshots { get; set; }
        public int AUGRightLegshots { get; set; }
        public int SG553RightLegshots { get; set; }
        public int GalilARRightLegshots { get; set; }
        public int FAMASRightLegshots { get; set; }
        public int AWPRightLegshots { get; set; }
        public int ScoutRightLegshots { get; set; }
        public int Scar20RightLegshots { get; set; }
        public int G3SG1RightLegshots { get; set; }
        public int SSG08RightLegshots { get; set; }
        public int DeagleRightLegshots { get; set; }
        public int GlockRightLegshots { get; set; }
        public int USPSRightLegshots { get; set; }
        public int P250RightLegshots { get; set; }
        public int P2000RightLegshots { get; set; }
        public int FiveSevenRightLegshots { get; set; }
        public int Tec9RightLegshots { get; set; }
        public int CZ75RightLegshots { get; set; }
        public int DualiesRightLegshots { get; set; }
        public int RevolverRightLegshots { get; set; }
        public int NovaRightLegshots { get; set; }
        public int XM1014RightLegshots { get; set; }
        public int SawedOffRightLegshots { get; set; }
        public int MAG7RightLegshots { get; set; }
        public int Mac10RightLegshots { get; set; }
        public int MP9RightLegshots { get; set; }
        public int MP7RightLegshots { get; set; }
        public int UMP45RightLegshots { get; set; }
        public int P90RightLegshots { get; set; }
        public int BizonRightLegshots { get; set; }
        public int NegevRightLegshots { get; set; }
        public int M249RightLegshots { get; set; }
        
        public DateTime LastUpdated { get; set; }
    }
}