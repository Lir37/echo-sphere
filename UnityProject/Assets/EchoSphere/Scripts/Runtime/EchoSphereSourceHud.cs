using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private string _sourceHudTooltip = "";
        private float _sourceHudTooltipUntil;
        private int _sourceHudDragSlot = -1;
        private Rect _sourceHudTooltipRect;
        private float SourceHudWidth => _uiWidth <= 1100f ? Mathf.Min(_uiWidth * .48f, 380f) : Mathf.Clamp(_uiWidth * .455f, 164f, 380f);
        private Rect SourceHudLeft => new Rect(12f, 12f, SourceHudWidth, _uiWidth <= 1100f ? 288f : 280f);
        private Rect SourceHudRight => new Rect(_uiWidth - 12f - SourceHudWidth, 12f, SourceHudWidth, _uiWidth <= 1100f ? 288f : 280f);
        public IList<EnemyAgent2D> SourceRadarEnemies => _enemies;
        public void DrawSourceRadarDot(Vector2 position, float radius, Color color) => DrawSphereGuiFilledCircle(position, radius, color);
        private Rect SourceControlsRect
        {
            get
            {
                var picker = GetSpherePickerRect();
                var abilities = Mathf.Ceil(_activeAbilityHotkeys.Count / 2f) * 100f;
                return new Rect(picker.x, picker.y - 360f - abilities, picker.width, picker.height + 360f + abilities);
            }
        }
        private bool IsSourceHudPoint(Vector2 point) => (_tutorialStep >= 0 && SourceTutorialCard.Contains(point)) || SourceHudLeft.Contains(point) || SourceHudRight.Contains(point) || SourceControlsRect.Contains(SourceControlsAnchor + (point - SourceControlsAnchor) / SourceInterfaceScale) ||
            (!string.IsNullOrEmpty(_sourceHudTooltip) && _sourceHudTooltipRect.Contains(point));
        private void DrawSourceHudPanel(Rect rect)
        {
            SourceUiRect(rect, new Color32(6, 17, 29, 235));
            var edge = new Color(0.31f, 0.69f, 0.88f, 0.18f);
            var softEdge = new Color(0.31f, 0.69f, 0.88f, 0.08f);
            SourceUiRect(new Rect(rect.x, rect.y, rect.width, 1f), edge);
            SourceUiRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), softEdge);
            SourceUiRect(new Rect(rect.x, rect.y, 1f, rect.height), softEdge);
            SourceUiRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), softEdge);
            SourceUiRect(new Rect(rect.x + 1f, rect.y + 1f, rect.width * 0.28f, 1f), new Color(0.39f, 0.90f, 1f, 0.18f));
        }

        private void DrawSourceGameplayHud()
        {
            EnsureKnowledgeLoaded(); DrawSourceDamageNumbers();
            var left = SourceHudLeft; var right = SourceHudRight;
            DrawSourceHudPanel(left); DrawSourceHudPanel(right);
            var width = left.width - 18f;
            SourceUiLabel(new Rect(left.x + 12f, left.y + 12f, width * .5f, 28f), (IsRussianUi ? "Уровень " : "Level ") + _level, 16, "#dcecff", false, true);
            SourceUiLabel(new Rect(left.x + left.width * .5f, left.y + 12f, width * .5f, 28f), Mathf.CeilToInt(_playerCore.CurrentHp) + "/" + Mathf.CeilToInt(_playerCore.MaxHp), 15, "#dcecff", true, true);
            DrawProgressBar(new Rect(left.x + 12f, left.y + 58f, width, 9f), _xpRequired > 0 ? _xp / _xpRequired : 0f, new Color32(57, 216, 255, 255));
            DrawProgressBar(new Rect(left.x + 12f, left.y + 79f, width, 8f), _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp), new Color32(85, 230, 193, 255));
            SourceUiLabel(new Rect(left.x + 12f, left.y + 99f, width, 20f), (IsRussianUi ? "РЕЗОНАНС" : "RESONANCE") + "    " + Mathf.FloorToInt(_resonanceCharge) + "/100", 12, "#7f9bb8", false, true);
            DrawProgressBar(new Rect(left.x + 12f, left.y + 126f, width, 8f), _resonanceCharge / 100f, _resonanceCharge >= 100f ? new Color32(255, 184, 77, 255) : new Color32(57, 216, 255, 255));
            SourceUiLabel(new Rect(left.x + 12f, left.y + 155f, width, 24f), (IsRussianUi ? "СФЕРЫ " : "SPHERES ") + _spheres.Count + "/" + GetCurrentMaxSpheres() + (IsRussianUi ? "     ВОЛНА " : "     WAVE ") + _wave, 13, "#7f9bb8");
            SourceUiLabel(new Rect(left.x + 12f, left.y + 195f, width, 22f), (IsRussianUi ? "АКТИВНЫЕ СВЯЗИ " : "ACTIVE LINKS ") + (_networkState?.Links.Count ?? 0), 12, "#7f9bb8", false, true);
            DrawSourceFormationChips(left);
            _regionFieldVisuals.DrawSourceRadar(new Rect(right.xMax - 145f, right.y + 14f, 130f, 130f));
            SourceUiLabel(new Rect(right.x + 12f, right.y + 150f, right.width - 24f, 28f), "•  " + FormatTime(_runTime) + "  •", 17, "#dcecff", true, true);
            var regionId = _selectedRegion == EchoRegionId.SpectralRift ? "spectral_rift" : "resonance_basin";
            SourceUiLabel(new Rect(right.x + 12f, right.y + 195f, right.width - 24f, 22f), PlannerName(PlannerEntry("region", regionId)), 13, "#66dfc7", false, true);
            SourceUiLabel(new Rect(right.x + 12f, right.y + 222f, right.width - 24f, 20f), GetRegionPhaseLabel(), 13, "#e0eff5");
            SourceUiLabel(new Rect(right.x + 12f, right.y + 248f, right.width - 24f, 24f), GetSourcePocketName(_regionHudPocketId) + " · " + (_regionEndlessMode ? IsRussianUi ? "БЕСКОНЕЧНЫЙ" : "ENDLESS" : IsRussianUi ? "СТАБИЛИЗАЦИЯ" : "STABILIZATION"), 11, "#7a96aa");
            DrawSourceJoystick();
            var matrix = GUI.matrix; var anchor = SourceControlsAnchor;
            GUI.matrix = matrix * Matrix4x4.Translate(anchor) * Matrix4x4.Scale(new Vector3(SourceInterfaceScale, SourceInterfaceScale, 1f)) * Matrix4x4.Translate(-anchor);
            DrawSourceRunControls(); DrawSpherePicker(); GUI.matrix = matrix; DrawSpherePlacementOverlay();
            DrawSourceFlashText();
            DrawSourceNetworkTooltip();
        }
        private string GetSourcePocketName(string id)
        {
            var ids = _selectedRegion == EchoRegionId.SpectralRift ? new[] { "prism", "shoal", "ember", "silent", "null" } : new[] { "axis", "glass", "fracture", "hollow", "pressure" };
            var ru = _selectedRegion == EchoRegionId.SpectralRift ? new[] { "ПРИЗМАТИЧЕСКИЙ СДВИГ", "ЗЕРКАЛЬНАЯ МЕЛЬ", "ТЛЕЮЩИЙ ИЗЛОМ", "ТИХАЯ РЕШЁТКА", "НУЛЕВОЙ САД" } : new[] { "ОСЕВОЙ УЗЕЛ", "СТЕКЛЯННЫЙ ПОТОК", "РАЗЛОМ", "ПУСТОЙ КОНТУР", "ЗОНА ДАВЛЕНИЯ" };
            var en = _selectedRegion == EchoRegionId.SpectralRift ? new[] { "PRISM SHIFT", "MIRROR SHOAL", "EMBER FAULT", "SILENT LATTICE", "NULL GARDEN" } : new[] { "AXIS NODE", "GLASS FLOW", "FRACTURE", "HOLLOW CONTOUR", "PRESSURE FIELD" };
            var index = Array.IndexOf(ids, id); return index < 0 ? "" : IsRussianUi ? ru[index] : en[index];
        }
        private string SourceFormationName(SphereNetworkFormation type)
        {
            var ru = new[] { "", "ЛИНИЯ", "ТРЕУГОЛЬНИК", "КЛАСТЕР", "КВАДРАТ", "КОЛЬЦО", "РЕШЁТКА", "ФРАКТАЛ" };
            return IsRussianUi && (int)type < ru.Length ? ru[(int)type] : type.ToString().ToUpperInvariant();
        }
        private void DrawSourceFormationChips(Rect panel)
        {
            var formations = new[] { _networkState?.DominantFormation, _networkState?.SecondaryFormation };
            var rects = new[] { new Rect(panel.x + 12f, panel.y + 224f, (panel.width - 36f) * .5f, 40f), new Rect(panel.x + panel.width * .5f + 6f, panel.y + 224f, (panel.width - 36f) * .5f, 40f) };
            if (formations[0] == null && formations[1] == null) { SourceUiLabel(new Rect(panel.x + 12f, panel.y + 224f, panel.width - 24f, 28f), IsRussianUi ? "ФОРМАЦИЯ НЕ АКТИВНА" : "NO FORMATION", 12, "#7f9bb8"); return; }
            SourceUiLabel(new Rect(panel.center.x - 8f, panel.y + 224f, 16f, 40f), "+", 14, "#7f9bb8", true);
            for (var i = 0; i < formations.Length; i++)
            {
                if (formations[i] == null) continue;
                var color = !formations[i].Active ? "#ff8a9f" : i == 0 ? "#ffb84d" : "#39d8ff";
                SourceUiRect(rects[i], new Color(.05f, .11f, .18f)); SourceUiLabel(rects[i], SourceFormationName(formations[i].Type), 12, color, true, true);
                if (GUI.enabled && Event.current.type == EventType.MouseDown && rects[i].Contains(Event.current.mousePosition))
                { _sourceHudDragSlot = i; _sourceHudTooltip = formations[i].Type.ToString(); _sourceHudTooltipUntil = Time.unscaledTime + 5f; Event.current.Use(); }
                if (GUI.enabled && Event.current.type == EventType.MouseUp && _sourceHudDragSlot >= 0 && rects[i].Contains(Event.current.mousePosition))
                {
                    if (_sourceHudDragSlot != i && formations[0] != null && formations[1] != null)
                    { _preferredDominantFormation = formations[1].Type; _preferredSecondaryFormation = formations[0].Type; RefreshNetworkState(); _sourceHudTooltip = ""; }
                    _sourceHudDragSlot = -1; Event.current.Use();
                }
            }
            if (Event.current.type == EventType.MouseUp) _sourceHudDragSlot = -1;
        }
        private void DrawSourceRunControls()
        {
            var picker = GetSpherePickerRect(); var x = picker.x + 4f; var y = picker.y - 101f;
            var enabled = GUI.enabled; GUI.enabled = !_follow && !_paused && !_gameOver && !_levelUp && !_stellaRewardPending;
            var dash = _playerCore.DashCooldown > 0f ? Mathf.CeilToInt(_playerCore.DashCooldown) + "s" : "DASH";
            DrawSphereGuiFilledCircle(new Vector2(x + 45f, y + 45f), 45f, new Color32(255, 184, 77, 217));
            if (SourceUiButton(new Rect(x, y, 90f, 90f), "", 1, Color.clear, "#dcecff")) TrySourceDash(true);
            SourceUiLabel(new Rect(x, y + 12f, 90f, 48f), "ϟ", 24, "#dcecff", true, true);
            SourceUiLabel(new Rect(x, y + 57f, 90f, 22f), dash, 8, "#dcecff", true, true);
            GUI.enabled = enabled; y -= 8f;
            var row = 0; var column = 0;
            foreach (var key in SourceActiveAbilityKeys)
            {
                var found = false; var kind = RunUpgradeKind.Blast;
                foreach (var binding in _activeAbilityHotkeys) if (binding.Value == key) { kind = binding.Key; found = true; break; }
                if (!found) continue;
                var entry = PlannerEntry("ability-evolution", GetAbilityFinal(kind)) ?? PlannerEntry("ability-evolution", GetAbilityBranch(kind)) ?? PlannerEntry("active", SourceAbilityId(kind));
                var cooldown = GetActiveAbilityCooldown(kind); GUI.enabled = !_paused && !_gameOver && !_levelUp && !_stellaRewardPending && !(_follow && kind == RunUpgradeKind.Teleport);
                if (SourceUiButton(new Rect(x + column * 98f, y - 67f - row * 76f, 90f, 67f), PlannerName(entry) + "\n" + (cooldown > 0f ? Mathf.CeilToInt(cooldown) + "s" : IsRussianUi ? "ГОТОВО" : "READY"), 12, new Color32(13, 23, 38, 230), "#b6c9de")) ActivateActiveAbility(kind);
                GUI.enabled = enabled; column++; if (column == 2) { column = 0; row++; }
            }
            var abilityRows = Mathf.Ceil(_activeAbilityHotkeys.Count / 2f); y -= abilityRows * 76f + 67f;
            if (SourceUiButton(new Rect(x, y, 68f, 56f), _sphereMovementLocked ? (IsRussianUi ? "ЗАБЛОКИРОВАНО" : "LOCKED") : IsRussianUi ? "ПЕРЕМЕЩЕНИЕ" : "MOVE", 10, new Color32(13, 23, 38, 230), "#b6c9de")) _sphereMovementLocked = !_sphereMovementLocked;
            var nodes = new List<FormationNode>(); foreach (var sphere in _spheres) if (sphere != null) nodes.Add(new FormationNode(new Vec2(sphere.transform.position.x, sphere.transform.position.y), true));
            GUI.enabled = _follow || FormationFollowRules.CanActivate(new Vec2(_player.position.x, _player.position.y), nodes);
            if (SourceUiButton(new Rect(x + 76f, y, 68f, 56f), _follow ? (IsRussianUi ? "СЛЕД" : "FOLLOWING") : "FOLLOW" , 11, new Color32(13, 23, 38, 230), "#b6c9de")) SetFormationFollowMode(!_follow);
            GUI.enabled = enabled; y -= 84f;
            if (_tutorialStep < 0 && SourceUiButton(new Rect(x, y, 68f, 68f), "Ⅱ", 28, new Color32(13, 23, 38, 230), "#b6c9de")) SetUserPaused(!_userPaused);
            if (_follow) SourceUiLabel(new Rect(x + 105f, picker.y - 58f, picker.width - 105f, 32f), (IsRussianUi ? "НАГРУЗКА СЕТИ " : "NETWORK STRAIN ") + Mathf.RoundToInt(_formationStrain) + "%", 11, "#7f9bb8", false, true);
        }
        private void DrawSourceNetworkTooltip()
        {
            if (string.IsNullOrEmpty(_sourceHudTooltip) || Time.unscaledTime >= _sourceHudTooltipUntil) { _sourceHudTooltip = ""; return; }
            var type = (SphereNetworkFormation)Enum.Parse(typeof(SphereNetworkFormation), _sourceHudTooltip);
            var secondary = _networkState?.SecondaryFormation?.Type == type;
            var inactive = _networkState?.DominantFormation?.Type == type && !_networkState.DominantFormation.Active;
            var scale = inactive ? 0f : secondary ? .5f : 1f;
            var body = SourceNetworkTooltipText(type, scale, secondary);
            var width = Mathf.Min(290f, _uiWidth - 24f); var style = new GUIStyle(_sphereDescriptionStyle) { wordWrap = true, fontSize = 11 };
            var height = 56f + style.CalcHeight(new GUIContent(body), width - 24f);
            _sourceHudTooltipRect = new Rect(12f, SourceHudLeft.yMax + 6f, width, height);
            GUI.Box(_sourceHudTooltipRect, GUIContent.none, _box);
            SourceUiLabel(new Rect(24f, _sourceHudTooltipRect.y + 10f, width - 24f, 22f), SourceFormationName(type) + " · " + Mathf.RoundToInt(scale * 100f) + "%", 12, secondary ? "#39d8ff" : "#ffb84d", false, true);
            GUI.Label(new Rect(24f, _sourceHudTooltipRect.y + 39f, width - 24f, height - 44f), body, style);
            if (Event.current.type == EventType.MouseDown && _sourceHudTooltipRect.Contains(Event.current.mousePosition)) { _sourceHudTooltip = ""; Event.current.Use(); }
        }
        private string SourceNetworkTooltipText(SphereNetworkFormation type, float scale, bool secondary)
        {
            var lines = new List<string>();
            string Percent(float value) => (value * scale).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
            void Add(string activeRu, string restRu, string activeEn, string restEn) { lines.Add(IsRussianUi ? activeRu + restRu : activeEn + restEn); }
  switch (type) {
    case SphereNetworkFormation.Line:
      Add("+" + Percent(10) + "%", secondary ? " из полного +10% урона попаданий через сеть" : " урона попаданий через активную сеть", "+" + Percent(10) + "%", secondary ? " of the full +10% network hit-damage bonus" : " hit damage through the active network");
      Add("+1", secondary ? " пробитие остаётся дискретным сетевым эффектом" : " пробитие для снарядов", "+1", secondary ? " pierce remains a discrete network effect" : " projectile pierce");
      if (CountSpheresOfType(SphereId.Prism) > 0) Add("+" + Percent(12) + "%", secondary ? " из полного +12% урона луча Призмы" : " урона луча Призмы", "+" + Percent(12) + "%", secondary ? " of the full +12% Prism beam damage" : " Prism beam damage");
      break;
    case SphereNetworkFormation.Triangle:
      Add(Percent(35) + "%", secondary ? " из полного 35% фактического урона сетевого импульса" : " фактического урона сетевого импульса", Percent(35) + "%", secondary ? " of the full 35% actual-damage network pulse" : " actual damage of the network pulse");
      Add("каждый 3-й удар", secondary ? " сферы создаёт импульс" : " сферы создаёт импульс · радиус 88", "every 3rd hit", secondary ? " creates the partial pulse" : " creates the pulse · radius 88");
      Add(secondary ? "50%" : "100%", secondary ? " эффективности вторичного слоя" : " эффективности доминирующего слоя", secondary ? "50%" : "100%", secondary ? " secondary-layer effectiveness" : " dominant-layer effectiveness");
      if (CountSpheresOfType(SphereId.Pulse) > 0 || CountSpheresOfType(SphereId.Prism) > 0) Add(secondary ? "50%" : "100%", " сетевого вклада в Резонанс через Треугольник", secondary ? "50%" : "100%", " network Resonance contribution through Triangle");
      break;
    case SphereNetworkFormation.Cluster:
      Add("-" + Percent(10) + "%", secondary ? " к интервалу атак вместо полного -10%" : " к интервалу атак сфер", "-" + Percent(10) + "%", secondary ? " Sphere attack interval instead of the full -10%" : " Sphere attack interval");
      if (CountSpheresOfType(SphereId.Orbital) > 0) Add("+" + Percent(8) + "%", secondary ? " к урону спутников вместо полного +8%" : " к урону спутников", "+" + Percent(8) + "%", secondary ? " satellite damage instead of the full +8%" : " satellite damage");
      if (CountSpheresOfType(SphereId.Gravity) > 0) Add("+" + Percent(20) + "%", secondary ? " к силе притяжения вместо полного +20%" : " к силе притяжения", "+" + Percent(20) + "%", secondary ? " pull strength instead of the full +20%" : " pull strength");
      if (CountSpheresOfType(SphereId.Pulse) > 0) Add("+" + Percent(6) + "%", secondary ? " к радиусу импульса вместо полного +6%" : " к радиусу импульса", "+" + Percent(6) + "%", secondary ? " pulse radius instead of the full +6%" : " pulse radius");
      break;
    case SphereNetworkFormation.Square:
      Add(secondary ? "8-й" : "4-й", " удар: +1 заряд щита", secondary ? "8th" : "4th", " hit: +1 shield charge");
      Add(secondary ? "50%" : "100%", " эффективности оборонительной формации", secondary ? "50%" : "100%", " defensive formation effectiveness");
      break;
    case SphereNetworkFormation.Ring:
      if (CountSpheresOfType(SphereId.Orbital) > 0) Add("+" + Percent(12) + "%", secondary ? " к радиусу орбиты вместо полного +12%" : " к радиусу орбиты", "+" + Percent(12) + "%", secondary ? " orbit radius instead of the full +12%" : " orbit radius");
      Add(secondary ? "50%" : "+1", secondary ? " шанс дополнительного отражённого луча" : " дополнительный отражённый луч", secondary ? "50%" : "+1", secondary ? " chance for the extra reflected beam" : " extra reflected beam");
      Add(secondary ? "50%" : "100%", " вклада Импульсного Резонатора в Резонанс", secondary ? "50%" : "100%", " Pulse Resonator network contribution to Resonance");
      break;
    case SphereNetworkFormation.Lattice:
      if (CountSpheresOfType(SphereId.Prism) > 0) Add("+" + Percent(8) + "%", secondary ? " к урону луча Призмы вместо полного +8%" : " к урону луча Призмы", "+" + Percent(8) + "%", secondary ? " Prism beam damage instead of the full +8%" : " Prism beam damage");
      Add(secondary ? "50%" : "100%", " вклада Импульсного Резонатора в Резонанс", secondary ? "50%" : "100%", " Pulse Resonator network contribution to Resonance");
      break;
    case SphereNetworkFormation.Fractal:
      Add("+" + Percent(15) + "%", secondary ? " к урону попаданий сфер вместо полного +15%" : " к урону попаданий сфер", "+" + Percent(15) + "%", secondary ? " Sphere hit damage instead of the full +15%" : " Sphere hit damage");
      break;
    default: break;
  }

            return string.Join("\n", lines);
        }
    }
}
