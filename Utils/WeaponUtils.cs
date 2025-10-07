using System.Collections.Generic;

namespace AdvancedStatistics.Utils
{
    public static class WeaponUtils
    {
        private static readonly Dictionary<string, string> WeaponNameMap = new()
        {
            // Rifles
            { "weapon_ak47", "ak47" },
            { "weapon_m4a1", "m4a4" },
            { "weapon_m4a1_silencer", "m4a1_silencer" },
            { "weapon_m4a1_silencer_off", "m4a1_silencer_off" },
            { "weapon_aug", "aug" },
            { "weapon_sg556", "sg556" },
            { "weapon_galilar", "galilar" },
            { "weapon_famas", "famas" },
            
            // Sniper rifles
            { "weapon_awp", "awp" },
            { "weapon_ssg08", "ssg08" },
            { "weapon_scar20", "scar20" },
            { "weapon_g3sg1", "g3sg1" },
            
            // Pistols
            { "weapon_deagle", "deagle" },
            { "weapon_glock", "glock" },
            { "weapon_usp_silencer", "usp_silencer" },
            { "weapon_usp_silencer_off", "usp_silencer_off" },
            { "weapon_p250", "p250" },
            { "weapon_hkp2000", "hkp2000" },
            { "weapon_fiveseven", "fiveseven" },
            { "weapon_tec9", "tec9" },
            { "weapon_cz75a", "cz75a" },
            { "weapon_elite", "elite" },
            { "weapon_revolver", "revolver" },
            
            // SMGs
            { "weapon_mac10", "mac10" },
            { "weapon_mp9", "mp9" },
            { "weapon_mp7", "mp7" },
            { "weapon_ump45", "ump45" },
            { "weapon_p90", "p90" },
            { "weapon_bizon", "bizon" },
            
            // Shotguns
            { "weapon_nova", "nova" },
            { "weapon_xm1014", "xm1014" },
            { "weapon_sawedoff", "sawedoff" },
            { "weapon_mag7", "mag7" },
            
            // Machine guns
            { "weapon_negev", "negev" },
            { "weapon_m249", "m249" },
            
            // Grenades and others (opcional para estadísticas)
            { "weapon_hegrenade", "hegrenade" },
            { "weapon_flashbang", "flashbang" },
            { "weapon_smokegrenade", "smokegrenade" },
            { "weapon_molotov", "molotov" },
            { "weapon_incgrenade", "incgrenade" },
            { "weapon_decoy", "decoy" },
        };

        public static string NormalizeWeaponName(string weaponEntityName)
        {
            if (string.IsNullOrEmpty(weaponEntityName))
                return "unknown";

            // Remover el prefijo "weapon_" si existe
            if (weaponEntityName.StartsWith("weapon_"))
            {
                weaponEntityName = weaponEntityName.Substring(7);
            }

            // Buscar en el mapa de nombres
            if (WeaponNameMap.TryGetValue($"weapon_{weaponEntityName}", out var normalized))
            {
                return normalized;
            }

            // Si no se encuentra, devolver el nombre tal cual
            return weaponEntityName;
        }

        public static bool IsWeaponTracked(string weaponName)
        {
            // Solo rastrear armas principales (no granadas, cuchillo, etc.)
            var trackedCategories = new[]
            {
                "ak47", "m4a4", "m4a1_silencer", "aug", "sg556", "galilar", "famas",
                "awp", "ssg08", "scar20", "g3sg1",
                "deagle", "glock", "usp_silencer", "p250", "hkp2000", "fiveseven", "tec9", "cz75a", "elite", "revolver",
                "mac10", "mp9", "mp7", "ump45", "p90", "bizon",
                "nova", "xm1014", "sawedoff", "mag7",
                "negev", "m249"
            };

            return System.Array.IndexOf(trackedCategories, weaponName.ToLower()) >= 0;
        }
    }
}