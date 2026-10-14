using System;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private Vector2 _sourceRegionScroll;
        private void StartSourceRegion(EchoRegionId region, string challenge, bool endless)
        { _selectedRegion = region; _regionChallengeId = challenge; _regionEndlessMode = endless; BeginSelectedRegionRun(); }
        private void DrawSourceRegionSelect()
        {
            EnsureKnowledgeLoaded();
            SourceUiRect(new Rect(0f, 0f, _uiWidth, _uiHeight), new Color32(2, 5, 13, 255));
            if (SourceUiButton(new Rect(12f, 14f, 37f, 34f), "‹", 25, new Color(.05f, .09f, .15f), "#dcecff")) _regionSelect = false;
            SourceUiLabel(new Rect(61f, 12f, _uiWidth - 73f, 30f), IsRussianUi ? "РЕГИОНЫ ЭХА" : "ECHO REGIONS", 20, "#dcecff", false, true);
            SourceUiLabel(new Rect(61f, 42f, _uiWidth - 73f, 19f), IsRussianUi ? "СТАБИЛИЗИРУЙ · ИСПЫТАЙ · УГЛУБИСЬ" : "STABILIZE · CHALLENGE · DESCEND", 9, "#7f9bb8");
            var width = Mathf.Min(760f, _uiWidth - 28f); var left = (_uiWidth - width) * .5f;
            var view = new Rect(left, 79f, width, _uiHeight - 92f);
            if (_sourceKnowledge?.planner == null) return;
            var regions = _sourceKnowledge.planner.FindAll(entry => entry.category == "region");
            var total = 92f;
            foreach (var region in regions) total += IsRegionAvailable(region.id == "spectral_rift" ? EchoRegionId.SpectralRift : EchoRegionId.ResonanceBasin) ? 489f : 168f;
            _sourceRegionScroll = GUI.BeginScrollView(view, _sourceRegionScroll, new Rect(0f, 0f, width - 18f, total));
            var contentWidth = width - 18f; var y = 0f;
            SourceUiLabel(new Rect(0f, y, contentWidth, 18f), IsRussianUi ? "ВЫБОР ОБЛАСТИ" : "REGION SELECT", 10, "#7f9bb8");
            SourceUiLabel(new Rect(0f, y + 22f, contentWidth, 28f), IsRussianUi ? "Один регион · несколько способов пройти его." : "One region · several ways to run it.", 13, "#dcecff", false, true);
            y += 64f;
            for (var i = 0; i < regions.Count; i++)
            {
                var entry = regions[i]; var region = entry.id == "spectral_rift" ? EchoRegionId.SpectralRift : EchoRegionId.ResonanceBasin;
                var available = IsRegionAvailable(region); var stable = available && IsRegionStabilized(region);
                var completed = GetCompletedRegionChallenges(region); var endless = available && IsRegionEndlessUnlocked(region);
                var cardHeight = available ? 477f : 156f;
                SourceUiRect(new Rect(0f, y, contentWidth, cardHeight), new Color32(8, 17, 28, 255));
                ColorUtility.TryParseHtmlString(entry.color, out var accent); SourceUiRect(new Rect(0f, y, 3f, cardHeight), accent);
                SourceUiLabel(new Rect(16f, y + 12f, 42f, 38f), "0" + (i + 1), 26, entry.color, true, true);
                SourceUiLabel(new Rect(71f, y + 11f, contentWidth - 158f, 18f), (IsRussianUi ? "РЕГИОН " : "REGION ") + "0" + (i + 1), 9, "#7f9bb8");
                SourceUiLabel(new Rect(71f, y + 32f, contentWidth - 82f, 32f), PlannerName(entry), 18, "#dcecff", false, true);
                SourceUiLabel(new Rect(71f, y + 67f, contentWidth - 82f, 26f), PlannerDesc(entry), 10, "#7f9bb8");
                SourceUiLabel(new Rect(contentWidth - 100f, y + 12f, 87f, 18f), available ? stable ? (IsRussianUi ? "СТАБИЛЕН" : "STABLE") : IsRussianUi ? "ДОСТУПЕН" : "AVAILABLE" : "🔒", 9, entry.color, true, true);
                SourceUiRect(new Rect(16f, y + 103f, contentWidth - 32f, 1f), new Color32(36, 59, 85, 255));
                if (!available)
                {
                    SourceUiLabel(new Rect(18f, y + 117f, contentWidth - 36f, 30f), IsRussianUi ? "Откроется после стабилизации Резонансного Бассейна." : "Unlocks after Resonance Basin is stabilized.", 11, "#7f9bb8");
                    y += cardHeight + 12f; continue;
                }
                var baseLabel = stable ? (IsRussianUi ? "ПОВТОРИТЬ СТАБИЛИЗАЦИЮ" : "REPLAY STABILIZATION") : IsRussianUi ? "СТАБИЛИЗАЦИЯ · 30:00" : "STABILIZATION · 30:00";
                var baseCopy = IsRussianUi ? "Базовый маршрут · открывает историю региона" : "Base route · opens the region story";
                if (SourceUiButton(new Rect(16f, y + 117f, contentWidth - 32f, 58f), baseLabel + "\n" + baseCopy + "  ›", 12, new Color(.06f, .12f, .18f), "#dcecff")) StartSourceRegion(region, "none", false);
                SourceUiLabel(new Rect(16f, y + 188f, contentWidth - 32f, 22f), (IsRussianUi ? "ИСПЫТАНИЯ" : "CHALLENGES") + " · " + completed.Length + "/3", 10, "#7f9bb8", false, true);
                for (var j = 0; j < entry.requires.Length; j++)
                {
                    var id = entry.requires[j]; var challenge = PlannerEntry("region-challenge", id);
                    var done = Array.IndexOf(completed, id) >= 0;
                    var status = done ? (IsRussianUi ? "ПРОЙДЕНО" : "CLEARED") : stable ? "›" : "🔒";
                    var oldEnabled = GUI.enabled; GUI.enabled = stable;
                    if (SourceUiButton(new Rect(16f, y + 218f + j * 63f, contentWidth - 32f, 55f), PlannerName(challenge) + " · " + status + "\n" + PlannerDesc(challenge), 11, new Color(.05f, .10f, .16f), "#b6c9de")) StartSourceRegion(region, id, false);
                    GUI.enabled = oldEnabled;
                }
                var endlessCopy = endless ? (IsRussianUi ? "Все три испытания пройдены · вход открыт" : "All three challenges cleared · access open") : (IsRussianUi ? "Закрыто · испытания " : "Locked · challenges ") + completed.Length + "/3";
                var enabled = GUI.enabled; GUI.enabled = endless;
                if (SourceUiButton(new Rect(16f, y + 411f, contentWidth - 32f, 53f), (IsRussianUi ? "∞  БЕСКОНЕЧНОСТЬ" : "∞  ENDLESS") + "\n" + endlessCopy, 12, new Color(.07f, .10f, .17f), "#b6c9de")) StartSourceRegion(region, "none", true);
                GUI.enabled = enabled; y += cardHeight + 12f;
            }
            SourceUiLabel(new Rect(0f, y + 8f, contentWidth, 30f), IsRussianUi ? "Каждая область меняет ритм врагов, события и давление на Сеть." : "Each region changes enemy rhythm, events and Network pressure.", 10, "#7f9bb8");
            GUI.EndScrollView();
        }
    }
}
