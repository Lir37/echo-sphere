using System.Collections.Generic;

namespace EchoSphere.Core
{
    public readonly struct ArtifactProtocolDefinition
    {
        public readonly string Id;
        public readonly string SetId;
        public readonly string NameRussian;
        public readonly string NameEnglish;
        public readonly string DescriptionRussian;
        public readonly string DescriptionEnglish;
        public readonly bool HasUnmappedRuntimeState;

        public ArtifactProtocolDefinition(
            string id, string setId, string nameRussian, string nameEnglish,
            string descriptionRussian, string descriptionEnglish, bool hasUnmappedRuntimeState)
        {
            Id = id;
            SetId = setId;
            NameRussian = nameRussian;
            NameEnglish = nameEnglish;
            DescriptionRussian = descriptionRussian;
            DescriptionEnglish = descriptionEnglish;
            HasUnmappedRuntimeState = hasUnmappedRuntimeState;
        }
    }

    /// <summary>
    /// Source-mapped Artifact Protocol combat multipliers from
    /// src/artifactSystem.ts:getSphereArtifactDamageMultiplier().
    /// The prerequisites are passed from source-equivalent set/build state.
    /// </summary>
    public static class ArtifactProtocolRules
    {
        private static readonly ArtifactProtocolDefinition[] ProtocolItems =
        {
            new ArtifactProtocolDefinition("triangle_resonance", "resonance_grid", "Треугольный резонанс", "Triangle Resonance",
                "Активируется при завершённой Резонансной решётке и рабочем треугольнике.",
                "Activates with a completed Resonance Grid and an active triangle.", false),
            new ArtifactProtocolDefinition("sphere_relay", "echo_architecture", "Релейный контур", "Sphere Relay",
                "+5% урона связанных сфер при наличии 2+ типов сфер.",
                "+5% damage for linked Spheres when 2+ Sphere types are present.", false),
            new ArtifactProtocolDefinition("critical_echo", "singularity_path", "Критическое Эхо", "Critical Echo",
                "+6% урона при комбо x5 и выше.",
                "+6% damage at combo x5 or higher.", false),
            new ArtifactProtocolDefinition("geometry_circuit", "geometry_craft", "Контур геометрии", "Geometry Circuit",
                "+5% урона при активной геометрии Кольца, Решётки или Фрактала.",
                "+5% damage while Ring, Lattice or Fractal geometry is active.", false),
            new ArtifactProtocolDefinition("void_horizon_protocol", "void_horizon", "Протокол Пустоты", "Void Horizon Protocol",
                "Активируется, когда сборка Пустотной сталкивается с элитой или боссом.",
                "Activates when a Void build has an Elite or Boss target.", false),
            new ArtifactProtocolDefinition("temporal_fold_protocol", "temporal_fold", "Временной сгиб", "Temporal Fold Protocol",
                "+6% урона сфер во время локального контроля времени.",
                "+6% Sphere damage during local time control.", false),
            new ArtifactProtocolDefinition("status_circuit_protocol", "status_circuit", "Протокол статусов", "Status Circuit Protocol",
                "+6% урона Призмы в статусной сборке.",
                "+6% Prism damage in a status build.", false),
            new ArtifactProtocolDefinition("hunter_doctrine_protocol", "hunter_doctrine", "Протокол охоты", "Hunter Doctrine Protocol",
                "+8% урона Снайперской при наличии элитной цели или босса.",
                "+8% Sniper damage when an Elite/Boss target is present.", false),
            new ArtifactProtocolDefinition("pulse_engineering_protocol", "pulse_engineering", "Импульсный протокол", "Pulse Engineering Protocol",
                "+8% урона Импульсной против группы из 4+ живых врагов.",
                "+8% Pulse damage against a group of 4+ living enemies.", false),
            new ArtifactProtocolDefinition("core_forge_protocol", "core_forge", "Протокол кузницы", "Core Forge Protocol",
                "+5% урона при 3+ активных типах сфер.",
                "+5% damage with 3+ active Sphere types.", false),
            new ArtifactProtocolDefinition("prism_dominion_protocol", "prism_dominion", "Протокол преломления", "Refraction Protocol",
                "+6% урона Призмы, Орбитальной и Пустотной при наличии Призмы и другой специализированной сферы.",
                "+6% Prism/Orbital/Void damage when Prism is paired with another specialized Sphere.", false),
            new ArtifactProtocolDefinition("network_legacy_protocol", "network_legacy", "Протокол наследия", "Legacy Protocol",
                "+6% урона при 3+ типах сфер и Кольце, Решётке или Фрактале.",
                "+6% damage with 3+ Sphere types and Ring/Lattice/Fractal.", false)
        };

        public static IReadOnlyList<ArtifactProtocolDefinition> Protocols => ProtocolItems;

        public static bool TryGetProtocol(string id, out ArtifactProtocolDefinition protocol)
        {
            for (var i = 0; i < ProtocolItems.Length; i++)
            {
                if (string.Equals(ProtocolItems[i].Id, id, System.StringComparison.Ordinal))
                {
                    protocol = ProtocolItems[i];
                    return true;
                }
            }
            protocol = default(ArtifactProtocolDefinition);
            return false;
        }

        public static int HitCadence(string id){switch(id){
            case "triangle_resonance":return 6;
            case "sphere_relay":return 8;
            case "critical_echo":return 10;
            case "geometry_circuit":return 10;
            case "void_horizon_protocol":return 6;
            case "temporal_fold_protocol":return 8;
            case "status_circuit_protocol":return 6;
            case "hunter_doctrine_protocol":return 5;
            case "pulse_engineering_protocol":return 8;
            case "core_forge_protocol":return 12;
            case "prism_dominion_protocol":return 8;
            case "network_legacy_protocol":return 30;
            default:return 0;}}
        public static string ModernDescription(ArtifactProtocolDefinition p,bool ru,bool modern){
            string original=ru?p.DescriptionRussian:p.DescriptionEnglish;if(!modern)return original;
            switch(p.Id){
                case "triangle_resonance":return original+" "+(ru?"После каждых 6 подходящих попаданий: +5 заряда Резонанса.":"Every 6 qualifying hits: +5 Resonance charge.");
                case "sphere_relay":return original+" "+(ru?"После каждых 8 подходящих попаданий: следующая атака атакующей сферы на 0,2 с раньше.":"Every 8 qualifying hits: advance the attacking Sphere’s next attack by 0.2s.");
                case "critical_echo":return original+" "+(ru?"После каждых 10 подходящих попаданий: восстановление 2 HP.":"Every 10 qualifying hits: restore 2 HP.");
                case "geometry_circuit":return original+" "+(ru?"После каждых 10 подходящих попаданий: +8 заряда Резонанса.":"Every 10 qualifying hits: +8 Resonance charge.");
                case "void_horizon_protocol":return original+" "+(ru?"После каждых 6 попаданий Пустотной по элите или боссу: замедление цели на 30% на 1 с.":"Every 6 Void hits on an Elite or Boss: slow the target by 30% for 1s.");
                case "temporal_fold_protocol":return original+" "+(ru?"После каждых 8 попаданий во время контроля времени: заморозка цели на 0,25 с.":"Every 8 hits during time control: freeze the target for 0.25s.");
                case "status_circuit_protocol":return original+" "+(ru?"После каждых 6 попаданий Призмы: замедление цели на 30% на 1 с.":"Every 6 Prism hits: slow the target by 30% for 1s.");
                case "hunter_doctrine_protocol":return original+" "+(ru?"После каждых 5 попаданий Снайперской по элите или боссу: дополнительный удар на 25% урона попадания.":"Every 5 Sniper hits on an Elite or Boss: an additional strike for 25% of hit damage.");
                case "pulse_engineering_protocol":return original+" "+(ru?"После каждых 8 попаданий Импульсной: отталкивание цели на 0,2 ед. мира.":"Every 8 Pulse hits: knock the target back by 0.2 world units.");
                case "core_forge_protocol":return original+" "+(ru?"После каждых 12 подходящих попаданий: восстановление 2 HP.":"Every 12 qualifying hits: restore 2 HP.");
                case "prism_dominion_protocol":return original+" "+(ru?"После каждых 8 попаданий Призмы или Орбитальной: следующая атака атакующей сферы на 0,2 с раньше.":"Every 8 Prism or Orbital hits: advance the attacking Sphere’s next attack by 0.2s.");
                case "network_legacy_protocol":return original+" "+(ru?"После каждых 30 подходящих попаданий: +1 заряд щита, до лимита 5.":"Every 30 qualifying hits: +1 shield charge, up to the 5-charge cap.");
                default:return original;}}
        public static bool IsProtocolActive(bool discovered, bool conditionSatisfied) =>
            discovered && conditionSatisfied;

        public static float GetTriangleResonanceMultiplier(bool completedGrid, bool sphereFormsTriangle) =>
            completedGrid && sphereFormsTriangle ? 1.05f : 1f;

        public static float GetSphereRelayMultiplier(
            bool completedEchoArchitecture, int uniqueTypeCount, bool hasNearbySphere) =>
            completedEchoArchitecture && uniqueTypeCount >= 2 && hasNearbySphere ? 1.05f : 1f;

        public static bool IsCriticalEchoActive(bool completedSingularityPath, int combo) =>
            completedSingularityPath && combo >= 5;

        public static float GetCriticalEchoMultiplier(bool completedSingularityPath, int combo) =>
            IsCriticalEchoActive(completedSingularityPath, combo) ? 1.06f : 1f;

        public static float GetTemporalFoldProtocolMultiplier(
            bool completedTemporalFold, bool timeControlActive) =>
            completedTemporalFold && timeControlActive ? 1.06f : 1f;

        public static float GetStatusCircuitProtocolMultiplier(
            bool completedStatusCircuit, bool isPrism, bool hasStatusBuild) =>
            completedStatusCircuit && isPrism && hasStatusBuild ? 1.06f : 1f;

        public static float GetGeometryCircuitMultiplier(
            bool completedGeometryCraft, SphereNetworkFormation geometryType) =>
            completedGeometryCraft && ArtifactSetRules.IsRingLatticeOrFractal(geometryType) ? 1.05f : 1f;

        public static float GetVoidHorizonProtocolMultiplier(
            bool completedVoidHorizon, bool isVoidSphere, bool hasLivingEliteOrBoss) =>
            completedVoidHorizon && isVoidSphere && hasLivingEliteOrBoss ? 1.08f : 1f;

        public static float GetHunterDoctrineProtocolMultiplier(
            bool completedHunterDoctrine, bool isSniperSphere, bool hasLivingEliteOrBoss) =>
            completedHunterDoctrine && isSniperSphere && hasLivingEliteOrBoss ? 1.08f : 1f;

        public static float GetPulseEngineeringProtocolMultiplier(
            bool completedPulseEngineering, bool isPulseSphere, int livingEnemyCount) =>
            completedPulseEngineering && isPulseSphere && livingEnemyCount >= 4 ? 1.08f : 1f;

        public static float GetCoreForgeProtocolMultiplier(bool completedCoreForge, int uniqueTypeCount) =>
            completedCoreForge && uniqueTypeCount >= 3 ? 1.05f : 1f;

        public static float GetPrismDominionProtocolMultiplier(
            bool completedPrismDominion, bool isPrismOrbitalOrVoid, bool refractionPressure) =>
            completedPrismDominion && isPrismOrbitalOrVoid && refractionPressure ? 1.06f : 1f;

        public static float GetNetworkLegacyProtocolMultiplier(
            bool completedNetworkLegacy, int uniqueTypeCount, SphereNetworkFormation geometryType) =>
            completedNetworkLegacy && uniqueTypeCount >= 3 &&
            ArtifactSetRules.IsRingLatticeOrFractal(geometryType) ? 1.06f : 1f;
    }
}
