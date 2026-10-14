using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private Vector2 _sourceSettingsScroll;
        private void DrawSourceSettings()
        {
            SourceUiRect(new Rect(0f, 0f, _uiWidth, _uiHeight), new Color32(2, 5, 13, 255));
            var width = Mathf.Min(400f, _uiWidth - 48f); var x = (_uiWidth - width) * .5f;
            if (SourceUiButton(new Rect(x, 22f, 120f, 32f), IsRussianUi ? "← Назад" : "← Back", 13, Color.clear, "#7f9bb8")) { _mainMenuPage = MainMenuPage.Home; _settingsResetPending = false; }
            SourceUiLabel(new Rect(x, 66f, width, 36f), IsRussianUi ? "Настройки" : "Settings", 24, "#dcecff", true, true);
            var rowWidth = width - 18f;
            var controls = IsRussianUi ? "Свайп/джойстик — движение\nТап по полю — разместить/снять сферу\nКнопки справа/слева — активные способности\nКнопка ⚡ — рывок (неуязвимость)" : "Swipe/joystick — move\nTap the field — place/remove sphere\nButtons on the side — active abilities\n⚡ button — dash (invulnerability)";
            var style = new GUIStyle(_sphereDescriptionStyle) { fontSize = 13, wordWrap = true }; var controlHeight = style.CalcHeight(new GUIContent(controls), rowWidth - 32f) + 32f;
            _sourceSettingsScroll = GUI.BeginScrollView(new Rect(x, 121f, width, _uiHeight - 137f), _sourceSettingsScroll, new Rect(0f, 0f, width - 18f, 572f + controlHeight + (_settingsResetPending ? 97f : 0f)));
            var half = (rowWidth - 8f) * .5f; var y = 0f;
            void Header(string ru, string en) { SourceUiLabel(new Rect(0f, y, rowWidth, 20f), IsRussianUi ? ru : en, 12, "#7f9bb8"); y += 28f; }
            Color Fill(bool selected) => selected ? new Color(.22f, .85f, 1f, .2f) : new Color32(13, 23, 38, 255);
            Header("Язык", "Language");
            if (SourceUiButton(new Rect(0f, y, half, 44f), "Русский", 13, Fill(IsRussianUi), IsRussianUi ? "#dcecff" : "#7f9bb8")) { PlayerPrefs.SetString(LanguagePreferenceKey, "ru"); PlayerPrefs.Save(); }
            if (SourceUiButton(new Rect(half + 8f, y, half, 44f), "English", 13, Fill(!IsRussianUi), !IsRussianUi ? "#dcecff" : "#7f9bb8")) { PlayerPrefs.SetString(LanguagePreferenceKey, "en"); PlayerPrefs.Save(); } y += 68f;
            Header("Звук", "Sound"); var sound = PlayerPrefs.GetInt(SoundPreferenceKey, 1) != 0;
            for (var i = 0; i < 2; i++) if (SourceUiButton(new Rect(i * (half + 8f), y, half, 44f), i == 0 ? IsRussianUi ? "Вкл" : "On" : IsRussianUi ? "Выкл" : "Off", 13, Fill(sound == (i == 0)), "#dcecff"))
            { PlayerPrefs.SetInt(SoundPreferenceKey, i == 0 ? 1 : 0); SourceAudio2D.SetEnabled(i == 0); PlayerPrefs.Save(); } y += 68f;
            Header("Расположение управления", "Control layout");
            for (var i = 0; i < 2; i++) if (SourceUiButton(new Rect(i * (half + 8f), y, half, 44f), i == 0 ? IsRussianUi ? "Правша" : "Right-handed" : IsRussianUi ? "Левша" : "Left-handed", 13, Fill(IsLeftHanded == (i == 1)), "#dcecff"))
            { PlayerPrefs.SetString(HandednessPreferenceKey, i == 0 ? "right" : "left"); PlayerPrefs.Save(); ClearSourcePointers(); } y += 68f;
            Header("Масштаб интерфейса", "Interface scale"); var scale = SourceInterfaceScale;
            SourceUiLabel(new Rect(0f, y, 36f, 25f), "75%", 12, "#7f9bb8"); SourceUiLabel(new Rect(rowWidth - 40f, y, 40f, 25f), "125%", 12, "#7f9bb8");
            var chosen = Mathf.Round(GUI.HorizontalSlider(new Rect(42f, y + 8f, rowWidth - 90f, 22f), scale, .75f, 1.25f) / .05f) * .05f;
            if (!Mathf.Approximately(scale, chosen)) { PlayerPrefs.SetFloat("echosphere_interface_scale", chosen); PlayerPrefs.Save(); }
            SourceUiLabel(new Rect(0f, y + 28f, rowWidth, 24f), Mathf.RoundToInt(chosen * 100f) + "%", 13, "#dcecff", true, true); y += 76f;
            Header("Управление", "Controls");
            SourceUiRect(new Rect(0f, y, rowWidth, controlHeight), new Color32(13, 23, 38, 255)); GUI.Label(new Rect(16f, y + 16f, rowWidth - 32f, controlHeight - 32f), controls, style); y += controlHeight + 24f;
            if (SourceUiButton(new Rect(0f, y, rowWidth, 44f), IsRussianUi ? "▷ Повторить обучение" : "▷ Replay tutorial", 13, new Color(.22f, .85f, 1f, .05f), "#9fefff"))
            { PlayerPrefs.SetInt(TutorialCompletedPreferenceKey, 0); PlayerPrefs.Save(); _tutorialStep = 0; _tutorialUpgradeOpened = false; _mainMenuPage = MainMenuPage.Home; } y += 68f;
            if (SourceUiButton(new Rect(0f, y, rowWidth, 44f), IsRussianUi ? "Сбросить прогресс" : "Reset progress", 13, new Color(1f, .3f, .36f, .1f), "#ff4d5d")) _settingsResetPending = true;
            if (_settingsResetPending)
            {
                y += 55f; SourceUiLabel(new Rect(0f, y, rowWidth, 44f), IsRussianUi ? "Сбросить весь прогресс (золото, улучшения, рекорды)?" : "Reset all progress (gold, upgrades, records)?", 13, "#dcecff"); y += 48f;
                if (SourceUiButton(new Rect(0f, y, half, 34f), IsRussianUi ? "Отмена" : "Cancel", 13, Fill(false), "#7f9bb8")) _settingsResetPending = false;
                if (SourceUiButton(new Rect(half + 8f, y, half, 34f), IsRussianUi ? "OK" : "OK", 13, Fill(false), "#ff4d5d")) { ResetPersistentProgress(); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
            }
            GUI.EndScrollView();
        }
    }
}
