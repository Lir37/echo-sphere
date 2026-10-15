using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // The four tabs and their contents follow PausePlanner in src/App.tsx.
        [Serializable] private sealed class SourcePlannerEntry
        {
            public string category, id, nameRu, nameEn, descRu, descEn;
            public string sphere, sphereBranch, ability, abilityFinal, color;
            public string[] levelsRu, levelsEn, requires;
        }
        private int _pausePlannerTab;
        private Vector2 _pausePlannerScroll;
        private string _pauseArtifactSetId;
        private string PlannerName(SourcePlannerEntry entry) => entry == null ? "" : IsRussianUi ? entry.nameRu : entry.nameEn;
        private string PlannerDesc(SourcePlannerEntry entry) => entry == null ? "" : IsRussianUi ? entry.descRu : entry.descEn;
        private SourcePlannerEntry PlannerEntry(string category, string id) => _sourceKnowledge?.planner?.Find(entry => entry.category == category && entry.id == id);
        private RunUpgradeKind PlannerAbility(string id)
        {
            foreach (RunUpgradeKind kind in Enum.GetValues(typeof(RunUpgradeKind))) if (SourceAbilityId(kind) == id) return kind;
            throw new ArgumentException("Unknown source ability: " + id);
        }
        private void DrawSourcePausePlanner()
        {
            EnsureKnowledgeLoaded();
            var contentEnabled = GUI.enabled;
            GUI.enabled = contentEnabled && string.IsNullOrEmpty(_pauseArtifactSetId);
            var previous = GUI.color; GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, _uiHeight), _uiPanelTexture); GUI.color = previous;
            var width = Mathf.Min(768f, _uiWidth - 24f); var height = Mathf.Min(_uiHeight * 0.92f, _uiHeight - 24f);
            var panel = new Rect((_uiWidth - width) * 0.5f, (_uiHeight - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none, _box);
            var character = CharacterRules.Get(_sphereCharacterId);
            GUI.Label(new Rect(panel.x + 16f, panel.y + 12f, width - 70f, 18f), IsRussianUi ? "Билд забега" : "Run Build", _sphereDescriptionStyle);
            GUI.Label(new Rect(panel.x + 16f, panel.y + 32f, width - 70f, 30f), (IsRussianUi ? "Пауза · " + character.NameRu : "Pause · " + character.NameEn), _menuTitle);
            GUI.Label(new Rect(panel.x + 16f, panel.y + 63f, width - 32f, 24f),
                (IsRussianUi ? character.RoleRu + " · уровень " : character.RoleEn + " · level ") + _level + (IsRussianUi ? " · Волна " : " · Wave ") + _wave, _sphereDescriptionStyle);
            if (GUI.Button(new Rect(panel.xMax - 49f, panel.y + 19f, 33f, 33f), "×", _button)) SetUserPaused(false);
            var labels = IsRussianUi ? new[] { "Показатели", "Способности", "Артефакты", "Синергии" } : new[] { "Stats", "Abilities", "Artifacts", "Synergies" };
            var tabWidth = (width - 32f) / 4f;
            var tabLabelStyle = new GUIStyle(_label) { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            for (var i = 0; i < 4; i++)
            {
                var tabRect = new Rect(panel.x + 16f + i * tabWidth, panel.y + 93f, tabWidth - 6f, 33f);
                previous = GUI.color; if (i == _pausePlannerTab) GUI.color = new Color(0.22f, 0.85f, 1f);
                if (GUI.Button(tabRect, GUIContent.none, _button))
                { _pausePlannerTab = i; _pausePlannerScroll = Vector2.zero; _pauseArtifactSetId = null; }
                GUI.color = previous;
                var iconColor = i == _pausePlannerTab ? new Color(0.22f, 0.85f, 1f, 1f) : new Color(0.50f, 0.63f, 0.75f, 1f);
                DrawPauseTabIcon(new Rect(tabRect.x + 9f, tabRect.y + 8f, 18f, 18f), i, iconColor);
                tabLabelStyle.normal.textColor = i == _pausePlannerTab ? new Color(0.86f, 0.97f, 1f, 1f) : new Color(0.50f, 0.63f, 0.75f, 1f);
                GUI.Label(new Rect(tabRect.x + 29f, tabRect.y + 1f, tabRect.width - 34f, tabRect.height - 2f), labels[i], tabLabelStyle);
            }
            var view = new Rect(panel.x + 16f, panel.y + 139f, width - 32f, height - 207f);
            var cards = new List<KeyValuePair<string, string>>();
            if (_pausePlannerTab == 0) BuildPauseStats(cards);
            else if (_pausePlannerTab == 1) BuildPauseAbilities(cards);
            else if (_pausePlannerTab == 2) BuildPauseArtifacts(cards);
            else BuildPauseSynergies(cards);
            var titleStyle = new GUIStyle(_label) { fontStyle = FontStyle.Bold, fontSize = 14, wordWrap = true };
            var bodyStyle = new GUIStyle(_sphereDescriptionStyle) { fontSize = 11, wordWrap = true };
            var statLabelStyle = new GUIStyle(_sphereDescriptionStyle) { fontSize = 10, alignment = TextAnchor.MiddleLeft };
            var statValueStyle = new GUIStyle(_label) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            var statGridCount = _pausePlannerTab == 0 ? Mathf.Min(8, cards.Count) : 0;
            var statWidth = (view.width - 18f - 18f) / 4f;
            var heights = new List<float>(); var total = statGridCount > 0 ? 98f : 0f; var cardWidth = view.width - 18f;
            for (var i = 0; i < cards.Count; i++)
            {
                if (i < statGridCount) { heights.Add(46f); continue; }
                var size = 30f + titleStyle.CalcHeight(new GUIContent(cards[i].Key), cardWidth - 24f) + bodyStyle.CalcHeight(new GUIContent(cards[i].Value), cardWidth - 24f);
                if (_pausePlannerTab == 2 && heights.Count > 0 && heights.Count <= _ownedArtifacts.Count) size += 65f + SourceArtifactBadgeHeight(_ownedArtifacts[heights.Count - 1], cardWidth - 24f);
                heights.Add(size); total += size + 9f;
            }
            _pausePlannerScroll = GUI.BeginScrollView(view, _pausePlannerScroll, new Rect(0f, 0f, cardWidth, total));
            var y = statGridCount > 0 ? 98f : 0f;
            for (var i = 0; i < cards.Count; i++)
            {
                if (i < statGridCount)
                {
                    var statRect = new Rect((i % 4) * (statWidth + 6f), (i / 4) * 52f, statWidth, 46f);
                    GUI.Box(statRect, GUIContent.none, _box);
                    GUI.Label(new Rect(statRect.x + 8f, statRect.y + 3f, statRect.width - 16f, 16f), cards[i].Key, statLabelStyle);
                    GUI.Label(new Rect(statRect.x + 8f, statRect.y + 19f, statRect.width - 16f, 24f), cards[i].Value, statValueStyle);
                    continue;
                }
                GUI.Box(new Rect(0f, y, cardWidth, heights[i]), GUIContent.none, _box);
                var titleHeight = titleStyle.CalcHeight(new GUIContent(cards[i].Key), cardWidth - 24f);
                GUI.Label(new Rect(12f, y + 10f, cardWidth - 24f, titleHeight), cards[i].Key, titleStyle);
                if (_pausePlannerTab == 2 && i > 0 && i <= _ownedArtifacts.Count)
                {
                    var artifact = ArtifactRewardRules.Get(_ownedArtifacts[i - 1]);
                    if (artifact != null) DrawSourceArtifactGlyph(new Rect(12f, y + titleHeight + 17f, 54f, 54f), artifact);
                    GUI.Label(new Rect(78f, y + titleHeight + 17f, cardWidth - 90f, 60f), cards[i].Value, bodyStyle);
                    DrawSourceArtifactBadges(_ownedArtifacts[i - 1], new Rect(12f, y + titleHeight + 80f, cardWidth - 24f, heights[i] - titleHeight - 86f));
                }
                else
                    GUI.Label(new Rect(12f, y + titleHeight + 17f, cardWidth - 24f, heights[i] - titleHeight - 20f), cards[i].Value, bodyStyle);
                y += heights[i] + 9f;
            }
            GUI.EndScrollView();
            if (GUI.Button(new Rect(panel.x + 16f, panel.yMax - 51f, (width - 38f) * 0.5f, 36f), IsRussianUi ? "Продолжить" : "Resume", _button)) SetUserPaused(false);
            if (GUI.Button(new Rect(panel.x + width * 0.5f + 3f, panel.yMax - 51f, (width - 38f) * 0.5f, 36f), IsRussianUi ? "Вернуться в меню" : "Return to menu", _button))
            { SaveCurrentRun(); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }

            GUI.enabled = contentEnabled;
            if (!string.IsNullOrEmpty(_pauseArtifactSetId)) DrawPauseArtifactSetDetails(panel);
        }
        private void DrawPauseTabIcon(Rect rect, int index, Color color)
        {
            var c = rect.center;
            if (index == 0)
            {
                DrawSphereGuiLine(new Vector2(rect.x + 1f, rect.yMax - 1f), new Vector2(rect.xMax - 1f, rect.yMax - 1f), color, 1.4f);
                DrawSphereGuiLine(new Vector2(rect.x + 3f, rect.yMax - 2f), new Vector2(rect.x + 3f, rect.y + 8f), color, 2f);
                DrawSphereGuiLine(new Vector2(rect.x + 8f, rect.yMax - 2f), new Vector2(rect.x + 8f, rect.y + 4f), color, 2f);
                DrawSphereGuiLine(new Vector2(rect.x + 13f, rect.yMax - 2f), new Vector2(rect.x + 13f, rect.y + 1f), color, 2f);
            }
            else if (index == 1)
            {
                DrawSphereGuiLine(new Vector2(c.x, rect.y), new Vector2(c.x, rect.yMax), color, 1.4f);
                DrawSphereGuiLine(new Vector2(rect.x, c.y), new Vector2(rect.xMax, c.y), color, 1.4f);
                DrawSphereGuiLine(new Vector2(rect.x + 3f, rect.y + 3f), new Vector2(rect.xMax - 3f, rect.yMax - 3f), color, 1.4f);
                DrawSphereGuiLine(new Vector2(rect.x + 3f, rect.yMax - 3f), new Vector2(rect.xMax - 3f, rect.y + 3f), color, 1.4f);
            }
            else if (index == 2)
            {
                DrawSphereGuiLine(new Vector2(rect.x + 2f, rect.y + 5f), new Vector2(c.x, rect.y + 1f), color, 1.4f);
                DrawSphereGuiLine(new Vector2(c.x, rect.y + 1f), new Vector2(rect.xMax - 2f, rect.y + 5f), color, 1.4f);
                DrawSphereGuiLine(new Vector2(rect.x + 2f, rect.y + 5f), new Vector2(rect.x + 2f, rect.yMax - 2f), color, 1.4f);
                DrawSphereGuiLine(new Vector2(rect.xMax - 2f, rect.y + 5f), new Vector2(rect.xMax - 2f, rect.yMax - 2f), color, 1.4f);
                DrawSphereGuiLine(new Vector2(rect.x + 2f, rect.yMax - 2f), new Vector2(rect.xMax - 2f, rect.yMax - 2f), color, 1.4f);
                DrawSphereGuiLine(new Vector2(c.x, rect.y + 3f), new Vector2(c.x, rect.yMax - 2f), color, 1.4f);
            }
            else
            {
                var a = new Vector2(rect.x + 3f, rect.y + 4f);
                var b = new Vector2(rect.xMax - 3f, rect.y + 4f);
                var d = new Vector2(c.x, rect.yMax - 3f);
                DrawSphereGuiLine(a, b, color, 1.4f);
                DrawSphereGuiLine(a, d, color, 1.4f);
                DrawSphereGuiLine(b, d, color, 1.4f);
                DrawSphereGuiCircle(a, 2.5f, color, 1.3f);
                DrawSphereGuiCircle(b, 2.5f, color, 1.3f);
                DrawSphereGuiCircle(d, 2.5f, color, 1.3f);
            }
        }

        private static void PauseCard(List<KeyValuePair<string, string>> cards, string title, string body) => cards.Add(new KeyValuePair<string, string>(title, body));
        private void BuildPauseStats(List<KeyValuePair<string, string>> cards)
        {
            var speed = 180f * GetArtifactMoveSpeedMultiplier() * LevelUpAbilityRules.GetMoveSpeedMultiplier(GetAbilityLevel(RunUpgradeKind.MoveSpeed)) * GetCharacterMoveSpeedMultiplier() * PlayerLoopRules.MutationSpeed(_mutationStage) *
                (_sphereCharacterId == SphereCharacterId.Berserker && _characterMasteryLevel >= 5 && _buffTimer > 0f ? 1.05f : 1f) * LevelUpAbilityRules.GetMomentumSpeedMultiplier(GetAbilityLevel(RunUpgradeKind.Momentum), _playerCore.DashTimer > 0f);
            var crit = Mathf.Min(CombatRules.CritHardCap, CombatRules.CritBase + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "critChance") + LevelUpAbilityRules.GetCritChanceBonus(GetAbilityLevel(RunUpgradeKind.Crit)) + PersistentUpgradeRules.GetCritChanceBonus(GetShopUpgradeLevel("crit")) + GetArchitectFormationCritBonus());
            var dodge = LevelUpAbilityRules.GetDodgeChance(GetAbilityLevel(RunUpgradeKind.Dodge), ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "dodgeChance"));
            var vampire = ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "vampire") + GetAbilityLevel(RunUpgradeKind.Vampire) * 0.03f;
            var xp = PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("xp")) + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "xpGain");
            PauseCard(cards, "HP", Mathf.CeilToInt(_playerCore.CurrentHp) + " / " + Mathf.CeilToInt(_playerCore.MaxHp));
            PauseCard(cards, IsRussianUi ? "Скорость" : "Speed", Mathf.FloorToInt(speed + 0.5f).ToString());
            PauseCard(cards, IsRussianUi ? "Крит" : "Crit", Mathf.FloorToInt(crit * 100f + 0.5f) + "%");
            PauseCard(cards, IsRussianUi ? "Уклонение" : "Dodge", Mathf.FloorToInt(dodge * 100f + 0.5f) + "%");
            PauseCard(cards, IsRussianUi ? "Вампиризм" : "Lifesteal", Mathf.FloorToInt(vampire * 100f + 0.5f) + "%");
            PauseCard(cards, IsRussianUi ? "Сфер" : "Spheres", _spheres.Count + " / " + GetCurrentMaxSpheres());
            PauseCard(cards, IsRussianUi ? "Убийства" : "Kills", _kills.ToString());
            PauseCard(cards, IsRussianUi ? "Множитель XP" : "XP Mult", Mathf.FloorToInt(xp * 100f + 0.5f) + "%");
            if (_sourceKnowledge?.planner == null) return;
            foreach (var entry in _sourceKnowledge.planner)
            {
                if (entry.category != "sphere") continue;
                var type = (SphereId)Enum.Parse(typeof(SphereId), entry.id, true);
                var live = _spheres.Find(sphere => sphere != null && sphere.Type == type);
                var count = CountSpheresOfType(type);
                var body = count > 0 ? (IsRussianUi ? "на поле ×" : "field ×") + count : IsRussianUi ? "не установлена" : "not placed";
                if (live == null) body += "\n" + PlannerDesc(entry);
                else
                {
                    var damage = GetSourceSphereAttackDamage(live) / SphereCombatRules.GetProfile(type).DamageMultiplier;
                    var delay = 1.2f / GetSphereAttackSpeedMultiplier(type) * (1f - .10f * GetFormationBonus(SphereNetworkFormation.Cluster, GetSphereIndex(live)));
                    var dpsDelay = Mathf.Max(.05f, delay * GetSourceSphereModifiers(type).delay * SphereCombatRules.GetProfile(type).DelayMultiplier);
                    body += (IsRussianUi ? "\nурон " : "\ndamage ") + Mathf.FloorToInt(damage + 0.5f) + (type == SphereId.Aura ? "/имп." : "") +
                        (IsRussianUi ? " · интервал " : " · delay ") + delay.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "с" +
                        (IsRussianUi ? "\nдальность " : "\nrange ") + Mathf.FloorToInt(live.AttackRange / SpherePlacementRules.SourcePixelsToWorld / SphereCombatRules.GetProfile(type).RangeMultiplier + 0.5f) + " · DPS " + PauseSphereDps(live, damage, dpsDelay).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                }
                var branch = GetSphereBranch(type); var final = GetSphereFinal(type);
                if (!string.IsNullOrEmpty(branch)) body += (IsRussianUi ? "\nВетка: " : "\nBranch: ") + PlannerName(PlannerEntry("sphere-branch", branch));
                if (!string.IsNullOrEmpty(final)) body += " → " + PlannerName(PlannerEntry("sphere-final", final));
                PauseCard(cards, PlannerName(entry) + (IsRussianUi ? " · ур. " : " · lvl ") + GetSphereProgressionLevel(type), body);
            }
        }
        private float PauseSphereDps(SphereAttackAgent sphere, float damage, float delay)
        {
            // getSphereDpsEstimate in src/engineSpheres.ts: present the same estimate, not measured damage.
            var profile = SphereCombatRules.GetProfile(sphere.Type);
            return sphere.GetSourceDpsEstimate(damage, Mathf.Max(0.05f, delay));
        }
        private void BuildPauseAbilities(List<KeyValuePair<string, string>> cards)
        {
            if (_sourceKnowledge?.planner == null) return;
            foreach (var category in new[] { "active", "passive" }) foreach (var entry in _sourceKnowledge.planner)
            {
                if (entry.category != category) continue;
                var kind = PlannerAbility(entry.id); var level = GetAbilityLevel(kind);
                if (category == "passive" && level <= 0) continue;
                var title = PlannerName(entry); var body = PlannerDesc(entry);
                var branch = PlannerEntry("ability-evolution", GetAbilityBranch(kind));
                var final = PlannerEntry("ability-evolution", GetAbilityFinal(kind));
                if (level > 0)
                {
                    var levels = IsRussianUi ? entry.levelsRu : entry.levelsEn;
                    if (levels != null && levels.Length >= level) body = levels[level - 1];
                    if (final != null) { title = PlannerName(final); body = PlannerDesc(final); }
                    else if (branch != null) { title = PlannerName(branch); body = PlannerDesc(branch); }
                }
                if (category == "active")
                {
                    body += "\nIV: " + (branch != null ? PlannerName(branch) : string.Join(" · ", AbilityPlannerChoices(entry.id, false)));
                    body += "\nVII: " + (final != null ? PlannerName(final) : string.Join(" · ", AbilityPlannerChoices(entry.id, true)));
                }
                PauseCard(cards, title + " · " + level + "/7", body);
            }
        }
        private List<string> AbilityPlannerChoices(string id, bool final)
        {
            var result = new List<string>();
            foreach (var entry in _sourceKnowledge.planner)
                if (entry.category == "ability-evolution" && entry.id.StartsWith(id + "_", StringComparison.Ordinal) && entry.id.Contains("_f") == final)
                { result.Add(PlannerName(entry)); if (result.Count == 3) break; }
            return result;
        }
        private void BuildPauseArtifacts(List<KeyValuePair<string, string>> cards)
        {
            PauseCard(cards, (IsRussianUi ? "Собрано: " : "Owned: ") + _ownedArtifacts.Count, _ownedArtifacts.Count == 0 ? (IsRussianUi ? "Артефактов пока нет." : "No artifacts yet.") : "");
            foreach (var id in _ownedArtifacts)
            {
                var artifact = ArtifactRewardRules.Get(id); if (artifact == null) continue;
                PauseCard(cards, (IsRussianUi ? artifact.NameRussian : artifact.NameEnglish) + " · " + SourceRarityName(artifact.Rarity),
                    IsRussianUi ? artifact.DescriptionRussian : artifact.DescriptionEnglish);
            }
        }
        private int PlannerRequirementsOwned(SourcePlannerEntry entry, bool synergies)
        {
            var count = 0;
            foreach (var id in entry.requires ?? Array.Empty<string>())
            {
                if (!synergies) { if (_ownedArtifacts.Contains(id)) count++; }
                else { var pair = PlannerEntry("artifact-synergy", id); if (pair != null && PlannerRequirementsOwned(pair, false) == pair.requires.Length) count++; }
            }
            return count;
        }
        private void BuildPauseSynergies(List<KeyValuePair<string, string>> cards)
        {
            if (_sourceKnowledge?.planner == null) return;
            foreach (var entry in _sourceKnowledge.planner)
            {
                if (entry.category != "sphere-synergy") continue;
                var sphere = (SphereId)Enum.Parse(typeof(SphereId), entry.sphere, true);
                var sphereOk = GetSphereProgressionLevel(sphere) >= 7; var abilityOk = GetAbilityLevel(PlannerAbility(entry.ability)) >= 7;
                var selected = GetSphereBranch(sphere) == entry.sphereBranch;
                // Source pause UI intentionally uses levels+branch, without requiring an ability final.
                var status = selected && sphereOk && abilityOk ? (IsRussianUi ? "АКТИВНА" : "ACTIVE") : selected ? (IsRussianUi ? "В ПРОЦЕССЕ" : "IN PROGRESS") : sphereOk ? (IsRussianUi ? "ДОСТУПНА" : "ELIGIBLE") : (IsRussianUi ? "ЗАКРЫТА" : "LOCKED");
                PauseCard(cards, PlannerName(entry) + " · " + status, PlannerDesc(entry) + "\n" + PlannerName(PlannerEntry("sphere", entry.sphere)) + " · " + PlannerName(PlannerEntry("sphere-branch", entry.sphereBranch)) + " · VII " + (sphereOk ? "✓" : "•") + " · " + PlannerName(PlannerEntry("active", entry.ability)) + " VII " + (abilityOk ? "✓" : "•"));
            }
            BuildPauseDiagnostics(cards);
            foreach (var entry in _sourceKnowledge.planner)
            {
                if (entry.category != "artifact-set") continue;
                var owned = PlannerRequirementsOwned(entry, true); var complete = owned == entry.requires.Length;
                PauseCard(cards, (complete ? PlannerName(entry) : "???") + " · " + owned + "/" + entry.requires.Length, complete ? PlannerDesc(entry) : IsRussianUi ? "Сет ещё не открыт." : "Set not discovered yet.");
            }
            foreach (var protocol in ArtifactProtocolRules.Protocols)
            {
                var discovered = IsArtifactProtocolSetComplete(protocol.SetId); var active = discovered && IsArtifactProtocolCurrentlyActive(protocol);
                PauseCard(cards, (discovered ? IsRussianUi ? protocol.NameRussian : protocol.NameEnglish : "???") + " · " + (active ? IsRussianUi ? "АКТИВЕН" : "ACTIVE" : discovered ? IsRussianUi ? "ОТКРЫТ" : "DISCOVERED" : "???"),
                    discovered ? IsRussianUi ? protocol.DescriptionRussian : protocol.DescriptionEnglish : IsRussianUi ? "Неизвестный протокол." : "Unknown protocol.");
            }
            foreach (var entry in _sourceKnowledge.planner) if (entry.category == "artifact-synergy")
            {
                var names = new List<string>(); foreach (var id in entry.requires) { var artifact = ArtifactRewardRules.Get(id); if (artifact != null) names.Add((_ownedArtifacts.Contains(id) ? "✓ " : "• ") + (IsRussianUi ? artifact.NameRussian : artifact.NameEnglish)); }
                PauseCard(cards, PlannerName(entry) + " · " + PlannerRequirementsOwned(entry, false) + "/" + entry.requires.Length, PlannerDesc(entry) + "\n" + string.Join(" · ", names));
            }
        }
        private void BuildPauseDiagnostics(List<KeyValuePair<string, string>> cards)
        {
            foreach (var group in new[] { new[] { "standard", "sniper", "shotgun", "chain", "pulse", "void" }, new[] { "aura", "gravity", "prism" } })
            {
                var signals = new List<string>(); foreach (var id in group) { var type = (SphereId)Enum.Parse(typeof(SphereId), id, true); if (GetSphereProgressionLevel(type) > 0) signals.Add(PlannerName(PlannerEntry("sphere", id)) + " " + GetSphereProgressionLevel(type)); }
                if (group[0] == "standard" && GetAbilityLevel(RunUpgradeKind.Damage) > 0) signals.Add(PlannerName(PlannerEntry("passive", "damage")) + " " + GetAbilityLevel(RunUpgradeKind.Damage));
                if (group[0] == "aura") { if (GetGlobalSphereModifierLevel("freeze") > 0) signals.Add("Freeze"); if (GetGlobalSphereModifierLevel("gravitic") > 0) signals.Add("Gravity"); }
                PauseCard(cards, group[0] == "standard" ? IsRussianUi ? "Атака" : "Offense" : IsRussianUi ? "Контроль" : "Control", signals.Count > 0 ? string.Join(" · ", signals) : IsRussianUi ? "Признаки пока не сформированы" : "No active signals yet");
            }
            var survival = new List<string>();
            var ids = new[] { "vitality", "shield", "dodge", "vampire", "movespeed" }; var ru = new[] { "Живучесть", "Барьер", "Уклонение", "Вампиризм", "Мобильность" }; var en = new[] { "Vitality", "Barrier", "Dodge", "Vampirism", "Mobility" };
            for (var i = 0; i < ids.Length; i++) if (GetAbilityLevel(PlannerAbility(ids[i])) > 0) survival.Add(IsRussianUi ? ru[i] : en[i]);
            PauseCard(cards, IsRussianUi ? "Выживание" : "Survival", survival.Count > 0 ? string.Join(" · ", survival) : IsRussianUi ? "Признаки пока не сформированы" : "No active signals yet");
            var types = new HashSet<SphereId>(); foreach (var sphere in _spheres) if (sphere != null) types.Add(sphere.Type);
            PauseCard(cards, IsRussianUi ? "Сеть" : "Network", (IsRussianUi ? "Типов сфер: " : "Sphere types: ") + types.Count + (IsRussianUi ? " · Связей: " : " · Links: ") + (_networkState?.Links.Count ?? 0) + (IsRussianUi ? " · Геометрия: " : " · Geometry: ") + _resonanceGeometryKey);
            PauseCard(cards, IsRussianUi ? "Резонанс" : "Resonance", (IsRussianUi ? "Резонанс: " : "Resonance: ") + Mathf.FloorToInt(_resonanceCharge) + "/100" + (IsRussianUi ? " · Геометрия: " : " · Geometry: ") + _resonanceGeometryKey);
            var synergySignals = new List<string>(); var sphereSynergyCount = 0;
            foreach (var entry in _sourceKnowledge.planner)
            {
                if (entry.category == "artifact-set" && PlannerRequirementsOwned(entry, true) == entry.requires.Length) synergySignals.Add(PlannerName(entry));
                if (entry.category == "sphere-synergy" && HasActiveAbilitySynergy((SphereId)Enum.Parse(typeof(SphereId), entry.sphere, true), entry.sphereBranch, PlannerAbility(entry.ability), entry.abilityFinal)) sphereSynergyCount++;
            }
            foreach (var protocol in ArtifactProtocolRules.Protocols) if (IsArtifactProtocolSetComplete(protocol.SetId) && IsArtifactProtocolCurrentlyActive(protocol)) synergySignals.Add(IsRussianUi ? protocol.NameRussian : protocol.NameEnglish);
            if (sphereSynergyCount > 0) synergySignals.Add((IsRussianUi ? "Активных синергий сфер: " : "Active Sphere synergies: ") + sphereSynergyCount);
            PauseCard(cards, IsRussianUi ? "Синергия" : "Synergy", synergySignals.Count > 0 ? string.Join(" · ", synergySignals) : IsRussianUi ? "Признаки пока не сформированы" : "No active signals yet");
        }
        private void DrawPauseArtifactSetButtons(Rect panel)
        {
            if (_sourceKnowledge?.planner == null) return;
            var sets = _sourceKnowledge.planner.FindAll(entry => entry.category == "artifact-set" && PlannerRequirementsOwned(entry, true) > 0);
            if (sets.Count == 0) return;
            var y = panel.y + 128f;
            foreach (var set in sets)
            {
                if (GUI.Button(new Rect(panel.xMax - 200f, y, 180f, 22f), PlannerName(set) + " · " + PlannerRequirementsOwned(set, true) + "/" + set.requires.Length, _button)) _pauseArtifactSetId = set.id;
                y += 23f; if (y > panel.yMax - 75f) break;
            }
        }
        private void DrawPauseArtifactSetDetails(Rect panel) => DrawSourceArtifactSetDetail();
    }
}
