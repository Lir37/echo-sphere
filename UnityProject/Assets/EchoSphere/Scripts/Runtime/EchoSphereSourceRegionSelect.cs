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
            if (SourceUiButton(new Rect(12f, 10f, 84f, 34f), "‹  " + (IsRussianUi ? "В МЕНЮ" : "MENU"), 7, new Color(.03f, .10f, .16f), "#7895a9")) _regionSelect = false;
            SourceUiLabel(new Rect(90f, 12f, _uiWidth - 180f, 20f), IsRussianUi ? "РЕГИОНЫ ЭХА" : "ECHO REGIONS", 13, "#dcecff", true, true);
            SourceUiLabel(new Rect(90f, 32f, _uiWidth - 180f, 12f), IsRussianUi ? "СТАБИЛИЗИРУЙ · ИСПЫТАЙ · УГЛУБИСЬ" : "STABILIZE · CHALLENGE · DESCEND", 6, "#7f9bb8", true);
            var width = Mathf.Min(720f, _uiWidth - 24f); var left = (_uiWidth - width) * .5f;
            var view = new Rect(left, 74f, width, _uiHeight - 92f);
            if (_sourceKnowledge?.Planner == null) return;
            var regions = _sourceKnowledge.Planner.FindAll(entry => entry.category == "region");
            var total = 64f;
            foreach (var region in regions) total += (IsRegionAvailable(region.id == "spectral_rift" ? EchoRegionId.SpectralRift : EchoRegionId.ResonanceBasin) ? 367f : 150f) + 12f;
            _sourceRegionScroll = GUI.BeginScrollView(view, _sourceRegionScroll, new Rect(0f, 0f, width, total));
            var contentWidth = width; var y = 0f;
            SourceUiLabel(new Rect(0f, y, contentWidth, 10f), IsRussianUi ? "ВЫБОР ОБЛАСТИ" : "REGION SELECT", 7, "#7f9bb8");
            SourceUiLabel(new Rect(0f, y + 13f, contentWidth, 16f), IsRussianUi ? "Один регион · несколько способов пройти его." : "One region · several ways to run it.", 10, "#dcecff", false, true);
            y += 36f;
            for (var i = 0; i < regions.Count; i++)
            {
                var entry = regions[i]; var region = entry.id == "spectral_rift" ? EchoRegionId.SpectralRift : EchoRegionId.ResonanceBasin;
                var available = IsRegionAvailable(region); var stable = available && IsRegionStabilized(region);
                var completed = GetCompletedRegionChallenges(region); var endless = available && IsRegionEndlessUnlocked(region);
                var cardHeight = available ? 367f : 150f;
                SourceUiRect(new Rect(0f, y, contentWidth, cardHeight), new Color32(8, 17, 28, 255));
                ColorUtility.TryParseHtmlString(entry.color, out var accent);
                SourceUiRect(new Rect(0f, y, 2f, cardHeight), accent);
                SourceUiLabel(new Rect(12f, y + 12f, 30f, 22f), "0" + (i + 1), 18, entry.color, true, true);
                SourceUiLabel(new Rect(52f, y + 12f, contentWidth - 150f, 9f), (IsRussianUi ? "РЕГИОН " : "REGION ") + "0" + (i + 1), 6, "#49697e");
                SourceUiLabel(new Rect(52f, y + 23f, contentWidth - 92f, 20f), PlannerName(entry), 15, "#dcecff", false, true);
                SourceUiLabel(new Rect(52f, y + 44f, contentWidth - 92f, 20f), PlannerDesc(entry), 7, "#7795aa");
                SourceUiLabel(new Rect(contentWidth - 96f, y + 12f, 84f, 12f), available ? stable ? (IsRussianUi ? "СТАБИЛЕН" : "STABLE") : IsRussianUi ? "ДОСТУПЕН" : "AVAILABLE" : "", 6, entry.color, true, true);
                if (!available) DrawRegionLockIcon(new Rect(contentWidth - 40f, y + 12f, 14f, 14f), new Color(.45f, .57f, .66f, .9f));
                SourceUiRect(new Rect(12f, y + 68f, contentWidth - 24f, 1f), new Color32(36, 59, 85, 255));
                if (!available)
                {
                    DrawRegionLockIcon(new Rect(14f, y + 91f, 14f, 14f), new Color(.45f, .57f, .66f, .9f));
                    SourceUiLabel(new Rect(36f, y + 82f, contentWidth - 48f, 50f), IsRussianUi ? "Откроется после стабилизации Резонансного Бассейна." : "Unlocks after Resonance Basin is stabilized.", 8, "#7f9bb8");
                    y += cardHeight + 12f; continue;
                }

                var baseLabel = stable ? (IsRussianUi ? "ПОВТОРИТЬ СТАБИЛИЗАЦИЮ" : "REPLAY STABILIZATION") : IsRussianUi ? "СТАБИЛИЗАЦИЯ · 30:00" : "STABILIZATION · 30:00";
                var baseCopy = IsRussianUi ? "Базовый маршрут · открывает историю региона" : "Base route · opens the region story";
                var baseRect = new Rect(12f, y + 80f, contentWidth - 24f, 52f);
                if (SourceUiButton(baseRect, "", 1, new Color(.06f, .12f, .18f), "#dcecff")) StartSourceRegion(region, "none", false);
                SourceUiRect(new Rect(baseRect.x, baseRect.y, 2f, baseRect.height), new Color(accent.r, accent.g, accent.b, .65f));
                SourceUiRect(new Rect(baseRect.x + 10f, baseRect.y + 10f, 32f, 32f), new Color(.02f, .08f, .13f, .9f));
                DrawSphereGuiLine(new Vector2(baseRect.x + 17f, baseRect.y + 15f), new Vector2(baseRect.x + 35f, baseRect.y + 32f), accent, 1.8f);
                DrawSphereGuiLine(new Vector2(baseRect.x + 35f, baseRect.y + 15f), new Vector2(baseRect.x + 17f, baseRect.y + 32f), accent, 1.8f);
                SourceUiLabel(new Rect(baseRect.x + 52f, baseRect.y + 6f, baseRect.width - 82f, 15f), baseLabel, 9, "#e8fbff", false, true);
                SourceUiLabel(new Rect(baseRect.x + 52f, baseRect.y + 24f, baseRect.width - 80f, 15f), baseCopy, 7, "#7593a7");
                DrawSphereGuiLine(new Vector2(baseRect.xMax - 22f, baseRect.center.y - 4f), new Vector2(baseRect.xMax - 16f, baseRect.center.y), new Color(.86f, .96f, 1f, 1f), 1.6f);
                DrawSphereGuiLine(new Vector2(baseRect.xMax - 16f, baseRect.center.y), new Vector2(baseRect.xMax - 22f, baseRect.center.y + 4f), new Color(.86f, .96f, 1f, 1f), 1.6f);

                SourceUiLabel(new Rect(12f, y + 143f, contentWidth - 24f, 10f), (IsRussianUi ? "ИСПЫТАНИЯ" : "CHALLENGES") + " · " + completed.Length + "/3", 7, "#648299", false, true);
                for (var j = 0; j < entry.requires.Length; j++)
                {
                    var id = entry.requires[j]; var challenge = PlannerEntry("region-challenge", id);
                    var done = Array.IndexOf(completed, id) >= 0;
                    var challengeAccent = Color.white;
                    ColorUtility.TryParseHtmlString(challenge?.color ?? entry.color, out challengeAccent);
                    var status = done ? (IsRussianUi ? "ПРОЙДЕНО" : "CLEARED") : stable ? "›" : "";
                    var oldEnabled = GUI.enabled; GUI.enabled = stable;
                    var challengeRect = new Rect(12f, y + 164f + j * 43f, contentWidth - 24f, 38f);
                    if (SourceUiButton(challengeRect, "", 1, new Color(.03f, .07f, .12f), "#b6c9de")) StartSourceRegion(region, id, false);
                    SourceUiRect(new Rect(challengeRect.x + 9f, challengeRect.y + 8f, 4f, 22f), challengeAccent);
                    SourceUiLabel(new Rect(challengeRect.x + 22f, challengeRect.y + 4f, challengeRect.width - 116f, 12f), PlannerName(challenge), 8, "#d7e9f4", false, true);
                    SourceUiLabel(new Rect(challengeRect.x + 22f, challengeRect.y + 17f, challengeRect.width - 116f, 16f), PlannerDesc(challenge), 7, "#718da2");
                    if (done) SourceUiLabel(new Rect(challengeRect.xMax - 92f, challengeRect.y + 11f, 82f, 14f), status, 6, "#" + ColorUtility.ToHtmlStringRGB(challengeAccent), true, true);
                    else if (stable) DrawSphereGuiLine(new Vector2(challengeRect.xMax - 22f, challengeRect.center.y - 4f), new Vector2(challengeRect.xMax - 16f, challengeRect.center.y), challengeAccent, 1.5f);
                    else DrawRegionLockIcon(new Rect(challengeRect.xMax - 25f, challengeRect.y + 11f, 12f, 14f), new Color(.45f, .57f, .66f, .9f));
                    GUI.enabled = oldEnabled;
                }

                var endlessCopy = endless ? (IsRussianUi ? "Все три испытания пройдены · вход открыт" : "All three challenges cleared · access open") : (IsRussianUi ? "Закрыто · испытания " : "Locked · challenges ") + completed.Length + "/3";
                var endlessRect = new Rect(12f, y + 303f, contentWidth - 24f, 50f);
                var endlessEnabled = GUI.enabled; GUI.enabled = endless;
                if (SourceUiButton(endlessRect, "", 1, new Color(.07f, .06f, .035f, .45f), "#eee5d1")) StartSourceRegion(region, "none", true);
                SourceUiRect(new Rect(endlessRect.x, endlessRect.y, 2f, endlessRect.height), new Color(1f, .77f, .35f, .4f));
                SourceUiLabel(new Rect(endlessRect.x + 10f, endlessRect.y + 3f, 32f, 42f), "∞", 20, "#ffd47c", true, true);
                SourceUiLabel(new Rect(endlessRect.x + 50f, endlessRect.y + 5f, endlessRect.width - 80f, 14f), IsRussianUi ? "БЕСКОНЕЧНОСТЬ" : "ENDLESS", 9, "#eee5d1", false, true);
                SourceUiLabel(new Rect(endlessRect.x + 50f, endlessRect.y + 22f, endlessRect.width - 80f, 19f), endlessCopy, 7, "#9b8662");
                if (endless) DrawSphereGuiLine(new Vector2(endlessRect.xMax - 22f, endlessRect.center.y - 4f), new Vector2(endlessRect.xMax - 16f, endlessRect.center.y), new Color(1f, .83f, .49f, 1f), 1.5f);
                else DrawRegionLockIcon(new Rect(endlessRect.xMax - 25f, endlessRect.y + 17f, 12f, 14f), new Color(.55f, .48f, .35f, .9f));
                GUI.enabled = endlessEnabled; y += cardHeight + 12f;
            }
            SourceUiLabel(new Rect(0f, y + 8f, contentWidth, 20f), IsRussianUi ? "Каждая область меняет ритм врагов, события и давление на Сеть." : "Each region changes enemy rhythm, events and Network pressure.", 7, "#48687d");
            GUI.EndScrollView();
        }

        private void DrawRegionLockIcon(Rect rect, Color color)
        {
            var c = rect.center;
            DrawSphereGuiCircle(new Vector2(c.x, rect.y + rect.height * .38f), rect.width * .23f, color, 1.2f);
            DrawSphereGuiLine(new Vector2(rect.x + rect.width * .22f, rect.y + rect.height * .38f), new Vector2(rect.x + rect.width * .22f, rect.yMax - 1f), color, 1.2f);
            DrawSphereGuiLine(new Vector2(rect.x + rect.width * .78f, rect.y + rect.height * .38f), new Vector2(rect.x + rect.width * .78f, rect.yMax - 1f), color, 1.2f);
            DrawSphereGuiLine(new Vector2(rect.x + rect.width * .22f, rect.yMax - 1f), new Vector2(rect.x + rect.width * .78f, rect.yMax - 1f), color, 1.2f);
        }
    }
}
