using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        [Serializable] private sealed class KnowledgeSave { public List<string> ids = new List<string>(); public List<BestiaryAppearance> appearances=new List<BestiaryAppearance>(); }
        private EchoSphere.Presentation.NativeArchiveCatalog _sourceKnowledge;
        private readonly HashSet<string> _knownKnowledgeIds = new HashSet<string>();
        private readonly Dictionary<string,BestiaryAppearance> _bestiaryAppearances=new Dictionary<string,BestiaryAppearance>(StringComparer.Ordinal);
        private bool _knowledgeLoaded;
        private string _knowledgeSection = "basics", _knowledgeQuery = "";
        private float _knowledgeTick;
        private static readonly string[] KnowledgeSections = { "basics", "spheres", "monsters", "bosses", "abilities", "artifacts", "evolutions", "synergies", "regions", "runes", "characters" };
        private static readonly string[] KnowledgeLabelsRu = { "Ядро игры", "Сферы", "Монстры", "Боссы", "Способности", "Артефакты", "Эволюции", "Синергии", "Регионы", "Руны", "Персонажи" };
        private static readonly string[] KnowledgeLabelsEn = { "Core", "Spheres", "Monsters", "Bosses", "Abilities", "Artifacts", "Evolutions", "Synergies", "Regions", "Runes", "Characters" };
        private void EnsureKnowledgeLoaded()
        {
            if (_knowledgeLoaded) return;
            _sourceKnowledge = Resources.Load<EchoSphere.Presentation.NativeArchiveCatalog>("EchoSphere/NativeArchiveCatalog");
            if (_sourceKnowledge == null) throw new InvalidOperationException("Native archive/planner catalog missing");
            try
            {
                var save = JsonUtility.FromJson<KnowledgeSave>(PlayerPrefs.GetString("echosphere_knowledge_v1", "{}"));
                if (save?.ids != null) foreach (var id in save.ids) _knownKnowledgeIds.Add(id);
                if(save?.appearances!=null)foreach(var appearance in save.appearances)if(appearance!=null&&!string.IsNullOrEmpty(appearance.Id)&&BestiaryCatalog.Find(appearance.Id)!=null)_bestiaryAppearances[appearance.Id]=appearance;
            }
            catch (Exception) { }
            _knowledgeLoaded = true;
        }
        private void RecordEnemyDefeat(EnemyAgent2D enemy)
        {
            if(enemy==null)return;EnsureKnowledgeLoaded();
            int region=enemy.CampaignRegionStyle>=0?enemy.CampaignRegionStyle:(int)ActiveCampaignRegion;
            string type=enemy.IsBoss?enemy.BossType:enemy.IsElite?enemy.EliteVariant:region==0?enemy.SourceVisualVariant:enemy.Role;
            string key=BestiaryCatalog.Key(region,enemy.IsBoss,enemy.IsElite,type);
            if(BestiaryCatalog.Find(key)==null)return;
            if(!_bestiaryAppearances.ContainsKey(key))_bestiaryAppearances[key]=new BestiaryAppearance{Id=key,Region=region,Role=enemy.Role,Variant=enemy.IsBoss?enemy.BossType:enemy.SourceVisualVariant,Boss=enemy.IsBoss,Elite=enemy.IsElite,Radius=enemy.SourceRadius,ColorHex=enemy.SourceColorHtml};
            RecordNaturalKnowledge(key);
        }
        private void RecordCompletedCharacterArchive()
        {
            if(_tutorialStep>=0||_runTime<=0)return;EnsureKnowledgeLoaded();
            RecordNaturalKnowledge("character:"+CharacterRules.ToSourceId(_sphereCharacterId));
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
            CollectRunKnowledge();
            if (_pendingKnowledgeRewards.Count > 0) PersistKnowledgeProgress();
        }
        // Discovery collection is independent of wallet/persistence, so fast encounters
        // and restored model state share the same identities and batched save.
        private void CollectRunKnowledge()
        {
            foreach (var sphere in _spheres) if (sphere != null) RecordNaturalKnowledge("sphere:" + sphere.Type.ToString().ToLowerInvariant());
            foreach (var pair in _sphereLevels) if (pair.Value > 0) RecordNaturalKnowledge("sphere:" + pair.Key.ToString().ToLowerInvariant());

            foreach (var pair in _abilityLevels) if (pair.Value > 0){string id=SourceAbilityId(pair.Key);if(EchoSphere.Presentation.NativeRunCatalog.Instance.GetAbility(id)?.Category!="active")RecordNaturalKnowledge("ability:"+id);}
            foreach (var artifact in _ownedArtifacts) RecordNaturalKnowledge("artifact:" + artifact);
            foreach (var pair in _sphereBranches) RecordNaturalKnowledge("sphere-branch:" + pair.Value);
            foreach (var pair in _sphereFinals)
            {
                if (!_sphereBranches.TryGetValue(pair.Key, out var branch) || string.IsNullOrEmpty(pair.Value)) continue;
                var suffix = pair.Value[pair.Value.Length - 1] - '1';
                RecordNaturalKnowledge("sphere-final:" + pair.Key.ToString().ToLowerInvariant() + ":" + branch + ":" + suffix);
            }
            foreach (var pair in _abilityBranches) RecordNaturalKnowledge("ability-evolution:" + pair.Value);
            foreach (var pair in _abilityFinals) RecordNaturalKnowledge("ability-evolution:" + pair.Value);
            foreach(var id in _fusedAbilityLinks)RecordNaturalKnowledge("synergy:"+id);
            foreach(var set in EchoSphere.Presentation.NativeArtifactBuilds.All){bool complete=true;foreach(var id in set.Requires)if(!_ownedArtifacts.Contains(id)){complete=false;break;}if(complete)RecordNaturalKnowledge(set.Id);}

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
