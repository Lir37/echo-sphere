using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private Vector2 _upgradePickerScroll;
        private SourcePlannerEntry SourceUpgradeText(RunUpgradeChoice choice)
        {
            EnsureKnowledgeLoaded();
            if (choice.Kind != RunUpgradeKind.Sphere) return PlannerEntry("ability-upgrade", SourceAbilityId(choice.Kind) + "|" + choice.NextLevel);
            var type = choice.Sphere.ToString().ToLowerInvariant();
            var branch = choice.NextLevel == 5 || choice.NextLevel == 6 ? GetSphereBranch(choice.Sphere) : "";
            return PlannerEntry("sphere-upgrade", type + "|" + choice.NextLevel + "|" + branch) ?? PlannerEntry("sphere-upgrade", type + "|" + choice.NextLevel + "|");
        }
        private string SourceUpgradeTitle(RunUpgradeChoice choice) { var entry = SourceUpgradeText(choice); return entry == null ? choice.Title : PlannerName(entry); }
        private string SourceUpgradeDescription(RunUpgradeChoice choice) { var entry = SourceUpgradeText(choice); return entry == null ? choice.Description : PlannerDesc(entry); }
        private SourcePlannerEntry SourceEvolutionText(SphereEvolutionOption choice)
        { EnsureKnowledgeLoaded(); return PlannerEntry(_pendingEvolutionIsAbility ? "ability-evolution" : "evolution-choice", choice.Id); }
        private string SourceEvolutionTitle(SphereEvolutionOption choice) { var entry = SourceEvolutionText(choice); return entry == null ? choice.Name : PlannerName(entry); }
        private string SourceEvolutionDescription(SphereEvolutionOption choice) { var entry = SourceEvolutionText(choice); return entry == null ? choice.Description : PlannerDesc(entry); }
        private void DrawSourceUpgradePicker()
        {
            var oldColor = GUI.color; GUI.color = new Color(0f, 0f, 0f, .8f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, _uiHeight), _uiPanelTexture); GUI.color = oldColor;
            var width = Mathf.Min(768f, _uiWidth - 24f); var height = Mathf.Min(_uiHeight * .92f, 620f);
            var panel = new Rect((_uiWidth - width) * .5f, (_uiHeight - height) * .5f, width, height);
            GUI.Box(panel, GUIContent.none, _box);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 14f, width - 36f, 32f), IsRussianUi ? "Выберите улучшение" : "Choose an upgrade", _menuTitle);
            if (_choosingEvolution) GUI.Label(new Rect(panel.x + 18f, panel.y + 47f, width - 36f, 22f), _pendingFinalEvolution ? "Mutation II" : "Mutation I", _label);
            var count = _choosingEvolution ? _evolutionChoices.Count : _levelUpChoices.Count;
            var bodyStyle = new GUIStyle(_sphereDescriptionStyle) { fontSize = 12, wordWrap = true };
            var titleStyle = new GUIStyle(_label) { fontSize = 15, fontStyle = FontStyle.Bold, wordWrap = true };
            var contentWidth = width - 54f;
            var heights = new float[count]; var names = new string[count]; var descriptions = new string[count]; var total = 0f;
            for (var i = 0; i < count; i++)
            {
                names[i] = _choosingEvolution ? SourceEvolutionTitle(_evolutionChoices[i]) : SourceUpgradeTitle(_levelUpChoices[i]);
                descriptions[i] = _choosingEvolution ? SourceEvolutionDescription(_evolutionChoices[i]) : SourceUpgradeDescription(_levelUpChoices[i]);
                heights[i] = Mathf.Max(86f, 30f + titleStyle.CalcHeight(new GUIContent(names[i]), contentWidth - 124f) + bodyStyle.CalcHeight(new GUIContent(descriptions[i]), contentWidth - 80f));
                total += heights[i] + 10f;
            }
            var view = new Rect(panel.x + 18f, panel.y + 77f, width - 36f, height - 133f);
            _upgradePickerScroll = GUI.BeginScrollView(view, _upgradePickerScroll, new Rect(0f, 0f, contentWidth, total));
            var y = 0f;
            for (var i = 0; i < count; i++)
            {
                var rect = new Rect(0f, y, contentWidth, heights[i]);
                var choosingEvolution = _choosingEvolution;
                var sphereChoice = choosingEvolution ? !_pendingEvolutionIsAbility : _levelUpChoices[i].Kind == RunUpgradeKind.Sphere;
                var type = choosingEvolution ? _pendingEvolutionSphere : _levelUpChoices[i].Sphere;
                var icon = sphereChoice ? GetSphereIcon(type) : RuntimeSpriteFactory.Faceted;
                var tint = sphereChoice ? SphereColor(type) : new Color(.7f, .62f, 1f);
                GUI.Box(rect, GUIContent.none, _box);
                var hit = GUI.Button(new Rect(rect.x, rect.y, rect.width - (choosingEvolution ? 0f : 58f), rect.height), GUIContent.none, _button);
                DrawSpriteIcon(new Rect(rect.x + 12f, rect.y + 12f, 42f, 42f), icon, tint);
                var titleHeight = titleStyle.CalcHeight(new GUIContent(names[i]), contentWidth - 124f);
                GUI.Label(new Rect(66f, y + 12f, contentWidth - 124f, titleHeight), names[i], titleStyle);
                GUI.Label(new Rect(66f, y + titleHeight + 19f, contentWidth - 80f, heights[i] - titleHeight - 23f), descriptions[i], bodyStyle);
                if (!choosingEvolution)
                {
                    var choiceKey = GetUpgradeChoiceKey(_levelUpChoices[i]);
                    var forecasted = _sphereCharacterId == SphereCharacterId.Oracle && _oracleForecastKeys.Contains(choiceKey);
                    var isLocked = _levelUpLockChoiceKey == choiceKey;
                    if (forecasted) GUI.Label(new Rect(contentWidth - 135f, y + 6f, 125f, 18f), IsRussianUi ? "ПРОГНОЗ" : "FORECAST", _sphereDescriptionStyle);
                    var lockRect = new Rect(contentWidth - 55f, y + 25f, 48f, 24f);
                    var enabled = GUI.enabled;
                    GUI.enabled = !isLocked && LevelUpChoiceRules.CanLockChoice(_levelUpChoices.Count, _levelUpLocksRemaining, !string.IsNullOrEmpty(_levelUpLockChoiceKey));
                    if (GUI.Button(lockRect, isLocked ? (IsRussianUi ? "ЗАКРЕП." : "LOCKED") : IsRussianUi ? "ЗАКРЕП." : "LOCK", _button)) { LockLevelUpChoice(i); hit = false; }
                    GUI.enabled = enabled;
                }
                y += heights[i] + 10f;
                if (hit) { if (choosingEvolution) ChooseEvolution(i); else ChooseUpgrade(i); _upgradePickerScroll = Vector2.zero; break; }
            }
            GUI.EndScrollView();
            if (!_choosingEvolution && _levelUpChoices.Count == 3)
            {
                var wasEnabled = GUI.enabled; GUI.enabled = LevelUpChoiceRules.CanRerollChoices(_levelUpChoices.Count, _levelUpRerollsRemaining);
                var rerollLabel = (IsRussianUi ? "ПЕРЕБРОСИТЬ" : "REROLL") + " · " + _levelUpRerollsRemaining;
                if (GUI.Button(new Rect(panel.x + 18f, panel.yMax - 44f, width - 36f, 30f), rerollLabel, _button)) { RerollLevelUpChoices(); _upgradePickerScroll = Vector2.zero; }
                GUI.enabled = wasEnabled;
            }
            if (_oracleSourceFlashTimer > 0f && !string.IsNullOrEmpty(_oracleSourceFlashText)) GUI.Label(new Rect(panel.x + 18f, panel.yMax - 72f, width - 36f, 24f), _oracleSourceFlashText, _label);
        }
    }
}
