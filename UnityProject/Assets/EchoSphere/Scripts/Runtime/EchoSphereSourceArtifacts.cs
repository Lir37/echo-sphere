using SourcePlannerEntry = EchoSphere.Presentation.NativeArchiveCatalog.PlannerEntry;
using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private Vector2 _sourceArtifactScroll, _sourceSetDetailScroll;
        private string SourceRarityName(ArtifactRarity rarity) => IsRussianUi ? new[] { "Обычный", "Редкий", "Эпический", "Особый", "Легендарный" }[(int)rarity] : rarity.ToString();
        private Color SourceRarityColor(ArtifactRarity rarity) => rarity == ArtifactRarity.Rare ? new Color32(57, 216, 255, 255) : rarity == ArtifactRarity.Epic ? new Color32(169, 140, 255, 255) : rarity == ArtifactRarity.Special ? new Color32(255, 107, 107, 255) : rarity == ArtifactRarity.Legendary ? new Color32(255, 184, 77, 255) : new Color32(127, 155, 184, 255);
        private List<string> SourceSetArtifacts(SourcePlannerEntry set)
        {
            var result = new List<string>();
            foreach (var pairId in set.requires ?? Array.Empty<string>())
                foreach (var id in PlannerEntry("artifact-synergy", pairId)?.requires ?? Array.Empty<string>()) if (!result.Contains(id)) result.Add(id);
            return result;
        }
        private List<SourcePlannerEntry> SourceSetsForArtifact(string id)
        { return _sourceKnowledge?.Planner?.FindAll(set => set.category == "artifact-set" && SourceSetArtifacts(set).Contains(id)) ?? new List<SourcePlannerEntry>(); }
        private int SourceSetOwned(SourcePlannerEntry set)
        { var count = 0; foreach (var id in SourceSetArtifacts(set)) if (_ownedArtifacts.Contains(id)) count++; return count; }
        private void DrawSourceArtifactGlyph(Rect rect, ArtifactRewardDefinition artifact)
        {
            var color = SourceRarityColor(artifact.Rarity); var center = rect.center; var radius = rect.width * .5f;
            DrawSphereGuiFilledCircle(center, radius, new Color32(7, 17, 29, 255));
            DrawSphereGuiCircle(center, radius, new Color(color.r, color.g, color.b, .53f), 1f);
            void Square(float inset, float angle, float alpha)
            {
                var half = rect.width * (.5f - inset); var points = new Vector2[4];
                for (var i = 0; i < 4; i++) { var a = (45f + angle + i * 90f) * Mathf.Deg2Rad; points[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * half * Mathf.Sqrt(2f); }
                for (var i = 0; i < 4; i++) DrawSphereGuiLine(points[i], points[(i + 1) % 4], new Color(1f, 1f, 1f, alpha), 1f);
            }
            Square(.11f, 45f, .08f); Square(.24f, 15f, .10f);
            DrawSphereGuiDashedCircle(center, rect.width * .33f, new Color(color.r, color.g, color.b, .33f), 1f, 3f, 3f);
            var sum = 0; foreach (var ch in artifact.Id) if (char.IsLetterOrDigit(ch)) sum += ch;
            var glyphs = new[] { "◇", "✦", "◈", "△", "⬡", "✧", "⊙", "✺", "⌬", "✹", "◌", "⟡" };
            var old = GUI.color; GUI.color = color;
            GUI.Label(rect, glyphs[sum % glyphs.Length], new GUIStyle(_label) { fontSize = 23, alignment = TextAnchor.MiddleCenter }); GUI.color = old;
            var cut = new Vector2(rect.xMax - rect.width * .26f, rect.yMax - rect.height * .22f);
            var delta = new Vector2(Mathf.Cos(-35f * Mathf.Deg2Rad), Mathf.Sin(-35f * Mathf.Deg2Rad)) * rect.width * .14f;
            DrawSphereGuiLine(cut - delta, cut + delta, new Color(color.r, color.g, color.b, .45f), 2f);
        }
        private float SourceArtifactBadgeHeight(string id, float width) => Mathf.Ceil(SourceSetsForArtifact(id).Count / Mathf.Max(1f, Mathf.Floor(width / 188f))) * 48f;
        private void DrawSourceArtifactBadges(string id, Rect rect)
        {
            var columns = Mathf.Max(1, Mathf.FloorToInt(rect.width / 188f)); var sets = SourceSetsForArtifact(id);
            for (var i = 0; i < sets.Count; i++)
            {
                var set = sets[i]; var owned = SourceSetOwned(set); var total = SourceSetArtifacts(set).Count;
                var badge = new Rect(rect.x + i % columns * rect.width / columns, rect.y + i / columns * 48f, rect.width / columns - 8f, 42f);
                if (GUI.Button(badge, GUIContent.none, _button)) { _pauseArtifactSetId = set.id; _sourceSetDetailScroll = Vector2.zero; }
                var center = new Vector2(badge.x + 19f, badge.center.y);
                DrawSphereGuiCircle(center, 16f, new Color32(36, 59, 85, 170), 3f);
                if (total > 0)
                {
                    var last = center + Vector2.up * 16f; var segments = Mathf.CeilToInt(48f * owned / total);
                    for (var j = 1; j <= segments; j++) { var a = -Mathf.PI * .5f + Mathf.PI * 2f * j / 48f; var next = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 16f; DrawSphereGuiLine(last, next, new Color32(255, 184, 77, 255), 3f); last = next; }
                }
                SourceUiLabel(new Rect(center.x - 15f, center.y - 9f, 30f, 18f), owned + "/" + total, 8, "#dcecff", true, true);
                SourceUiLabel(new Rect(badge.x + 42f, badge.y + 5f, badge.width - 45f, 17f), PlannerName(set), 8, "#dcecff", false, true);
                SourceUiLabel(new Rect(badge.x + 42f, badge.y + 23f, badge.width - 45f, 14f), (IsRussianUi ? "СЕТ · " : "SET · ") + (owned == total ? IsRussianUi ? "СОБРАН" : "COMPLETE" : IsRussianUi ? "ПРОГРЕСС" : "PROGRESS"), 7, "#7f9bb8");
            }
        }
        private string SourceArtifactMechanic(string id)
        {
            var ids = new[] { "swift", "mirror", "resonance", "lone", "fivefold", "relay", "triangle", "overclock", "network", "singularity", "zero", "unified" };
            var ru = new[] { "Темп", "Контратака", "Связь сфер", "Одна сфера", "Сеть", "Прогрессия", "Геометрия", "Разгон", "Сеть", "Сингулярность", "Архитектура", "Единый разум" };
            var en = new[] { "Tempo", "Counter", "Sphere Link", "Solo Sphere", "Network", "Progression", "Geometry", "Overclock", "Network", "Singularity", "Architecture", "Unified Mind" };
            var index = Array.IndexOf(ids, id); return index < 0 ? "" : IsRussianUi ? ru[index] : en[index];
        }
        private List<SourcePlannerEntry> SourceArtifactNewSynergies(string id)
        {
            return _sourceKnowledge?.Planner?.FindAll(pair => pair.category == "artifact-synergy" && Array.IndexOf(pair.requires, id) >= 0 && PlannerRequirementsOwned(pair, false) < pair.requires.Length && PlannerRequirementsOwned(pair, false) + 1 == pair.requires.Length) ?? new List<SourcePlannerEntry>();
        }
        private void DrawSourceArtifactChoices()
        {
            EnsureKnowledgeLoaded(); var modal = !string.IsNullOrEmpty(_pauseArtifactSetId); var enabled = GUI.enabled; GUI.enabled = enabled && !modal;
            SourceUiRect(new Rect(0f, 0f, _uiWidth, _uiHeight), new Color(0f, 0f, 0f, .7f));
            var width = Mathf.Min(1024f, _uiWidth - 48f); var x = (_uiWidth - width) * .5f;
            var columns = _uiWidth < 640f ? 1 : 3; var cardWidth = (width - (columns - 1) * 16f - 18f) / columns;
            var choices = _stellaRewardChoices.ToArray(); var heights = new float[choices.Length]; var rowHeights = new float[Mathf.CeilToInt(choices.Length / (float)columns)];
            var bodyStyle = new GUIStyle(_sphereDescriptionStyle) { fontSize = 10, wordWrap = true };
            for (var i = 0; i < choices.Length; i++)
            { heights[i] = Mathf.Max(106f, 89f + bodyStyle.CalcHeight(new GUIContent(IsRussianUi ? choices[i].DescriptionRussian : choices[i].DescriptionEnglish), cardWidth - 122f)) + 37f + SourceArtifactBadgeHeight(choices[i].Id, cardWidth - 32f) + SourceArtifactNewSynergies(choices[i].Id).Count * 50f + (string.IsNullOrEmpty(SourceArtifactMechanic(choices[i].Mechanic)) ? 0f : 32f); rowHeights[i / columns] = Mathf.Max(rowHeights[i / columns], heights[i]); }
            var total = 0f; foreach (var height in rowHeights) total += height + 16f;
            var panelHeight = Mathf.Min(_uiHeight - 32f, total + 90f); var top = (_uiHeight - panelHeight) * .5f;
            SourceUiLabel(new Rect(x, top, width, 32f), IsRussianUi ? "Артефакт" : "Artifact", 24, "#dcecff", true, true);
            SourceUiLabel(new Rect(x, top + 34f, width, 42f), IsRussianUi ? "Артефакты меняют правила взаимодействия сфер и персонажа" : "Artifacts change the rules of how spheres and the player interact", 13, "#7f9bb8", true);
            _sourceArtifactScroll = GUI.BeginScrollView(new Rect(x, top + 82f, width, panelHeight - 82f), _sourceArtifactScroll, new Rect(0f, 0f, width - 18f, total));
            var rowY = 0f;
            for (var i = 0; i < choices.Length; i++)
            {
                if (i > 0 && i % columns == 0) rowY += rowHeights[i / columns - 1] + 16f;
                var artifact = choices[i]; var rect = new Rect(i % columns * (cardWidth + 16f), rowY, cardWidth, heights[i]);
                var tint = SourceRarityColor(artifact.Rarity); SourceUiRect(rect, new Color32(13, 23, 38, 255));
                DrawSourceArtifactGlyph(new Rect(rect.x + 16f, rect.y + 16f, 74f, 74f), artifact);
                SourceUiLabel(new Rect(rect.x + 102f, rect.y + 16f, rect.width - 118f, 15f), SourceRarityName(artifact.Rarity), 9, "#" + ColorUtility.ToHtmlStringRGB(tint), false, true);
                SourceUiLabel(new Rect(rect.x + 102f, rect.y + 34f, rect.width - 118f, 40f), IsRussianUi ? artifact.NameRussian : artifact.NameEnglish, 16, "#dcecff", false, true);
                GUI.Label(new Rect(rect.x + 102f, rect.y + 77f, rect.width - 118f, heights[i] - 96f), IsRussianUi ? artifact.DescriptionRussian : artifact.DescriptionEnglish, bodyStyle);
                var y = rect.y + Mathf.Max(106f, 89f + bodyStyle.CalcHeight(new GUIContent(IsRussianUi ? artifact.DescriptionRussian : artifact.DescriptionEnglish), cardWidth - 122f));
                SourceUiLabel(new Rect(rect.x + 16f, y, rect.width - 32f, 23f), IsRussianUi ? "Связанные сборки" : "Build paths", 8, "#7f9bb8"); y += 28f;
                DrawSourceArtifactBadges(artifact.Id, new Rect(rect.x + 16f, y, rect.width - 32f, SourceArtifactBadgeHeight(artifact.Id, cardWidth - 32f))); y += SourceArtifactBadgeHeight(artifact.Id, cardWidth - 32f);
                foreach (var synergy in SourceArtifactNewSynergies(artifact.Id))
                { SourceUiLabel(new Rect(rect.x + 16f, y, rect.width - 32f, 18f), IsRussianUi ? "Активирует синергию" : "Activates synergy", 9, "#8064a8"); SourceUiLabel(new Rect(rect.x + 16f, y + 18f, rect.width - 32f, 24f), PlannerName(synergy), 12, "#8064a8", false, true); y += 50f; }
                SourceUiLabel(new Rect(rect.x + 16f, y, rect.width - 32f, 24f), SourceArtifactMechanic(artifact.Mechanic), 10, "#" + ColorUtility.ToHtmlStringRGB(tint));
                if (GUI.enabled && Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition)) { ChooseStellaReward(i); Event.current.Use(); }
            }
            GUI.EndScrollView(); GUI.enabled = enabled;
            if (!string.IsNullOrEmpty(_pauseArtifactSetId)) DrawSourceArtifactSetDetail();
        }
        private void DrawSourceArtifactSetDetail()
        {
            var set = PlannerEntry("artifact-set", _pauseArtifactSetId); if (set == null) return;
            SourceUiRect(new Rect(0f, 0f, _uiWidth, _uiHeight), new Color(0f, 0f, 0f, .75f));
            var width = Mathf.Min(448f, _uiWidth - 32f); var height = Mathf.Min(_uiHeight - 32f, 280f + SourceSetArtifacts(set).Count * 48f);
            var rect = new Rect((_uiWidth - width) * .5f, (_uiHeight - height) * .5f, width, height);
            SourceUiRect(rect, new Color32(7, 17, 29, 255));
            SourceUiLabel(new Rect(rect.x + 20f, rect.y + 16f, width - 40f, 18f), IsRussianUi ? "НАБОР АРТЕФАКТОВ" : "ARTIFACT SET", 9, "#ffb84d", false, true);
            SourceUiLabel(new Rect(rect.x + 20f, rect.y + 38f, width - 40f, 32f), PlannerName(set) + " · " + SourceSetOwned(set) + "/" + SourceSetArtifacts(set).Count, 18, "#dcecff", false, true);
            var style = new GUIStyle(_sphereDescriptionStyle) { wordWrap = true, fontSize = 13 }; var descHeight = style.CalcHeight(new GUIContent(PlannerDesc(set)), width - 64f);
            _sourceSetDetailScroll = GUI.BeginScrollView(new Rect(rect.x + 20f, rect.y + 78f, width - 40f, height - 139f), _sourceSetDetailScroll, new Rect(0f, 0f, width - 58f, 87f + descHeight + SourceSetArtifacts(set).Count * 48f));
            SourceUiLabel(new Rect(12f, 9f, width - 82f, 18f), IsRussianUi ? "ФИНАЛЬНЫЙ ЭФФЕКТ" : "COMPLETION EFFECT", 9, "#ffb84d", false, true);
            GUI.Label(new Rect(12f, 32f, width - 82f, descHeight), PlannerDesc(set), style);
            var y = 52f + descHeight; SourceUiLabel(new Rect(0f, y, width - 58f, 20f), IsRussianUi ? "АРТЕФАКТЫ НАБОРА" : "SET ARTIFACTS", 9, "#7f9bb8", false, true); y += 28f;
            foreach (var id in SourceSetArtifacts(set))
            {
                var artifact = ArtifactRewardRules.Get(id); if (artifact == null) continue; var owned = _ownedArtifacts.Contains(id);
                DrawSourceArtifactGlyph(new Rect(8f, y + 6f, 28f, 28f), artifact);
                SourceUiLabel(new Rect(44f, y + 7f, width - 146f, 28f), owned ? IsRussianUi ? artifact.NameRussian : artifact.NameEnglish : "???", 10, "#dcecff", false, true);
                SourceUiLabel(new Rect(width - 108f, y + 7f, 50f, 28f), owned ? IsRussianUi ? "ЕСТЬ" : "OWNED" : IsRussianUi ? "НЕТ" : "MISSING", 8, "#7f9bb8"); y += 48f;
            }
            GUI.EndScrollView();
            if (SourceUiButton(new Rect(rect.x + 20f, rect.yMax - 46f, width - 40f, 30f), IsRussianUi ? "Закрыть" : "Close", 10, new Color32(13, 23, 38, 255), "#b6c9de")) _pauseArtifactSetId = null;
            if (Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition)) { _pauseArtifactSetId = null; Event.current.Use(); }
        }
    }
}
