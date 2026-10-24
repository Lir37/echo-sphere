using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        [Serializable] private sealed class KnowledgeSave { public List<string> ids = new List<string>(); }
        private EchoSphere.Presentation.NativeArchiveCatalog _sourceKnowledge;
        private readonly HashSet<string> _knownKnowledgeIds = new HashSet<string>();
        private bool _knowledgeLoaded;
        private string _knowledgeSection = "basics", _knowledgeQuery = "";
        private float _knowledgeTick;
        private static readonly string[] KnowledgeSections = { "basics", "spheres", "monsters", "bosses", "abilities", "artifacts", "evolutions" };
        private static readonly string[] KnowledgeLabelsRu = { "Ядро игры", "Сферы", "Монстры", "Боссы", "Способности", "Артефакты", "Эволюции" };
        private static readonly string[] KnowledgeLabelsEn = { "Core", "Spheres", "Monsters", "Bosses", "Abilities", "Artifacts", "Evolutions" };
        private void EnsureKnowledgeLoaded()
        {
            if (_knowledgeLoaded) return;
            _sourceKnowledge = Resources.Load<EchoSphere.Presentation.NativeArchiveCatalog>("EchoSphere/NativeArchiveCatalog");
            if (_sourceKnowledge == null) throw new InvalidOperationException("Native archive/planner catalog missing");
            try
            {
                var save = JsonUtility.FromJson<KnowledgeSave>(PlayerPrefs.GetString("echosphere_knowledge_v1", "{}"));
                if (save?.ids != null) foreach (var id in save.ids) _knownKnowledgeIds.Add(id);
            }
            catch (Exception) { }
            _knowledgeLoaded = true;
        }
        private string SourceAbilityId(RunUpgradeKind kind)
        {
            switch (kind)
            {
                case RunUpgradeKind.MaxSpheres: return "maxspheres";
                case RunUpgradeKind.MoveSpeed: return "movespeed";
                case RunUpgradeKind.AttackSpeed: return "attackspeed";
                case RunUpgradeKind.TimeStop: return "timestop";
                case RunUpgradeKind.FireTrail: return "firetrail";
                case RunUpgradeKind.DarkRitual: return "darkritual";
                case RunUpgradeKind.SphereBoost: return "sphereboost";
                case RunUpgradeKind.CritPower: return "critpower";
                case RunUpgradeKind.ProjectileSpeed: return "proj_speed";
                default: return kind.ToString().ToLowerInvariant();
            }
        }
        private void TickSourceKnowledge(float dt)
        {
            _knowledgeTick += dt;
            if (_knowledgeTick < 0.35f && !_gameOver) return;
            _knowledgeTick = 0f; EnsureKnowledgeLoaded();
            var previous = _knownKnowledgeIds.Count;
            foreach (var sphere in _spheres) if (sphere != null) _knownKnowledgeIds.Add("sphere:" + sphere.Type.ToString().ToLowerInvariant());
            foreach (var pair in _sphereLevels) if (pair.Value > 0) _knownKnowledgeIds.Add("sphere:" + pair.Key.ToString().ToLowerInvariant());
            foreach (var enemy in _enemies) if (enemy != null) _knownKnowledgeIds.Add(enemy.IsBoss ? "boss:" + enemy.BossType : "monster:" + enemy.SourceMonsterType);
            foreach (var pair in _abilityLevels) if (pair.Value > 0) _knownKnowledgeIds.Add("ability:" + SourceAbilityId(pair.Key));
            foreach (var artifact in _ownedArtifacts) _knownKnowledgeIds.Add("artifact:" + artifact);
            foreach (var pair in _sphereBranches) _knownKnowledgeIds.Add("sphere-branch:" + pair.Value);
            foreach (var pair in _sphereFinals)
            {
                if (!_sphereBranches.TryGetValue(pair.Key, out var branch) || string.IsNullOrEmpty(pair.Value)) continue;
                var suffix = pair.Value[pair.Value.Length - 1] - '1';
                _knownKnowledgeIds.Add("sphere-final:" + pair.Key.ToString().ToLowerInvariant() + ":" + branch + ":" + suffix);
            }
            foreach (var pair in _abilityBranches) _knownKnowledgeIds.Add("ability-evolution:" + pair.Value);
            foreach (var pair in _abilityFinals) _knownKnowledgeIds.Add("ability-evolution:" + pair.Value);
            if (previous != _knownKnowledgeIds.Count)
            { PlayerPrefs.SetString("echosphere_knowledge_v1", JsonUtility.ToJson(new KnowledgeSave { ids = new List<string>(_knownKnowledgeIds) })); PlayerPrefs.Save(); }
        }
        private void DrawSourceKnowledgeBaseScreen()
        {
            EnsureKnowledgeLoaded();
            var panel = DrawMenuScreenFrame(IsRussianUi ? "БАЗА ЗНАНИЙ" : "KNOWLEDGE BASE",
                (IsRussianUi ? "ОТКРЫТО ЗАПИСЕЙ: " : "ENTRIES DISCOVERED: ") + _knownKnowledgeIds.Count, _uiHeight - 24f);
            var labels = IsRussianUi ? KnowledgeLabelsRu : KnowledgeLabelsEn;
            var tabWidth = (panel.width - 36f) / 4f;
            for (var i = 0; i < KnowledgeSections.Length; i++)
            {
                var old = GUI.color; if (_knowledgeSection == KnowledgeSections[i]) GUI.color = new Color(0.4f, 0.92f, 1f);
                if (GUI.Button(new Rect(panel.x + 18f + (i % 4) * tabWidth, panel.y + 86f + (i / 4) * 30f, tabWidth - 4f, 26f), labels[i], _button))
                { _knowledgeSection = KnowledgeSections[i]; _knowledgeScrollPosition = Vector2.zero; }
                GUI.color = old;
            }
            _knowledgeQuery = GUI.TextField(new Rect(panel.x + 18f, panel.y + 150f, panel.width - 36f, 28f), _knowledgeQuery, _button);
            var view = new Rect(panel.x + 18f, panel.y + 188f, panel.width - 36f, panel.height - 239f);
            if (_sourceKnowledge == null) return;
            var filtered = _sourceKnowledge.Entries.FindAll(entry => entry.section == _knowledgeSection &&
                (_knowledgeSection == "basics" || _knowledgeSection == "evolutions" || (IsRussianUi ? entry.nameRu : entry.nameEn).IndexOf(_knowledgeQuery.Trim(), StringComparison.OrdinalIgnoreCase) >= 0));
            var columns = _knowledgeSection == "basics" || panel.width < 640f ? 1 : 2;
            var cardWidth = (view.width - 18f - (columns - 1) * 10f) / columns;
            var textStyle = new GUIStyle(_sphereDescriptionStyle) { wordWrap = true, fontSize = 12 };
            var titleStyle = new GUIStyle(_label) { wordWrap = true, fontSize = 15, fontStyle = FontStyle.Bold };
            var heights = new List<float>(); var total = 0f;
            for (var i = 0; i < filtered.Count; i += columns)
            {
                var height = 0f;
                for (var j = i; j < Mathf.Min(i + columns, filtered.Count); j++)
                {
                    var e = filtered[j]; var unlocked = string.IsNullOrEmpty(e.id) || _knownKnowledgeIds.Contains(e.id);
                    var body = unlocked ? (IsRussianUi ? e.descRu + "\n" + e.detailsRu : e.descEn + "\n" + e.detailsEn) :
                        (IsRussianUi ? "Получите этот объект во время игры, чтобы открыть запись." : "Obtain or encounter this during a run to unlock the entry.");
                    height = Mathf.Max(height, 62f + textStyle.CalcHeight(new GUIContent(body), cardWidth - 26f));
                }
                heights.Add(height); total += height + 10f;
            }
            _knowledgeScrollPosition = GUI.BeginScrollView(view, _knowledgeScrollPosition, new Rect(0f, 0f, view.width - 18f, total));
            var y = 0f;
            for (var i = 0; i < filtered.Count; i++)
            {
                var e = filtered[i]; var height = heights[i / columns]; var unlocked = string.IsNullOrEmpty(e.id) || _knownKnowledgeIds.Contains(e.id);
                var card = new Rect((i % columns) * (cardWidth + 10f), y, cardWidth, height);
                GUI.Box(card, GUIContent.none, _box);
                GUI.Label(new Rect(card.x + 13f, card.y + 10f, card.width - 26f, 42f), unlocked ? (IsRussianUi ? e.nameRu : e.nameEn) : (IsRussianUi ? "Запись не открыта" : "Entry locked"), titleStyle);
                var body = unlocked ? (IsRussianUi ? e.descRu + "\n" + e.detailsRu : e.descEn + "\n" + e.detailsEn) :
                    (IsRussianUi ? "Получите этот объект во время игры, чтобы открыть запись." : "Obtain or encounter this during a run to unlock the entry.");
                GUI.Label(new Rect(card.x + 13f, card.y + 55f, card.width - 26f, height - 60f), body, textStyle);
                if (i % columns == columns - 1) y += height + 10f;
            }
            GUI.EndScrollView();
            if (GUI.Button(new Rect(panel.x + 22f, panel.yMax - 46f, 142f, 32f), IsRussianUi ? "НАЗАД" : "BACK", _button)) _mainMenuPage = MainMenuPage.Home;
        }
    }
}
