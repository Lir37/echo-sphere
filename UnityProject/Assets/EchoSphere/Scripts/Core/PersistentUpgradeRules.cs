using System;

namespace EchoSphere.Core
{
    /// <summary>Persistent shop cost/level rules sourced from new-desing/src/gameData.ts.</summary>
    public static class PersistentUpgradeRules
    {
        public static readonly string[] Ids = { "dmg", "radius", "speed", "spheres", "hp", "crit", "xp" };
        public static int GetBaseCost(string id)
        {
            switch (id)
            {
                case "dmg": case "radius": return 200;
                case "speed": case "xp": return 300;
                case "spheres": return 500;
                case "hp": return 150;
                case "crit": return 400;
                default: return 0;
            }
        }
        public static int GetMaxLevel(string id)
        {
            switch (id)
            {
                case "dmg": case "radius": case "hp": return 10;
                case "speed": case "crit": case "xp": return 7;
                case "spheres": return 3;
                default: return 0;
            }
        }
        public static int ClampLevel(string id, int level) => Math.Max(0, Math.Min(GetMaxLevel(id), level));
        public static int GetCost(string id, int currentLevel)
        {
            var max = GetMaxLevel(id);
            if (max <= 0 || currentLevel < 0 || currentLevel >= max) return 0;
            return (int)Math.Floor(GetBaseCost(id) * Math.Pow(3d, currentLevel));
        }
        public static float GetPercentMultiplier(int level) => 1f + Math.Max(0, level) * 0.05f;
        public static int GetStartingHpBonus(int level) => Math.Max(0, level) * 10;
        public static int GetExtraSpheres(int level) => Math.Max(0, Math.Min(3, level));
        public static float GetCritChanceBonus(int level) => Math.Max(0, Math.Min(7, level)) * 0.05f;
    }
}
