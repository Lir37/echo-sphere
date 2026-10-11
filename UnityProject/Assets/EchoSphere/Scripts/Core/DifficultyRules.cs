namespace EchoSphere.Core
{
    /// <summary>Current source Difficulty settings from new-desing/src/gameData.ts.</summary>
    public readonly struct DifficultyProfile
    {
        public readonly string Id, NameRu, NameEn;
        public readonly float EnemyHpMultiplier, EnemySpeedMultiplier, EnemyDamageMultiplier, GoldMultiplier, SpawnRateMultiplier;
        public DifficultyProfile(string id, string nameRu, string nameEn, float hp, float speed, float damage, float gold, float spawnRate)
        {
            Id = id; NameRu = nameRu; NameEn = nameEn;
            EnemyHpMultiplier = hp; EnemySpeedMultiplier = speed; EnemyDamageMultiplier = damage;
            GoldMultiplier = gold; SpawnRateMultiplier = spawnRate;
        }
    }
    public static class DifficultyRules
    {
        public static string Normalize(string id)
        {
            switch (id) { case "easy": case "normal": case "hard": case "nightmare": return id; default: return "normal"; }
        }
        public static DifficultyProfile GetProfile(string id)
        {
            switch (Normalize(id))
            {
                case "easy": return new DifficultyProfile("easy", "Лёгкая", "Easy", 0.7f, 0.8f, 0.7f, 0.8f, 0.8f);
                case "hard": return new DifficultyProfile("hard", "Сложная", "Hard", 1.4f, 1.2f, 1.3f, 1.5f, 1.2f);
                case "nightmare": return new DifficultyProfile("nightmare", "Кошмар", "Nightmare", 2f, 1.4f, 1.6f, 2f, 1.5f);
                default: return new DifficultyProfile("normal", "Обычная", "Normal", 1f, 1f, 1f, 1f, 1f);
            }
        }
    }
}
