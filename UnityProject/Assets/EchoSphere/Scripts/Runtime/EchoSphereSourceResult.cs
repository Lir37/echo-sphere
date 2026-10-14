using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _sourceResultNewRecord;
        private int _sourceResultRank;
        private void DrawSourceRunResult()
        {
            SourceUiRect(new Rect(0, 0, _uiWidth, _uiHeight), new Color(0f, 0f, 0f, .8f));
            var rows = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>(IsRussianUi ? "Вы прожили" : "You survived", Mathf.FloorToInt(_runTime) + (IsRussianUi ? " сек" : " sec")),
                new KeyValuePair<string, string>(IsRussianUi ? "Волна" : "Wave", _wave.ToString()),
                new KeyValuePair<string, string>(IsRussianUi ? "УБИЙСТВА" : "KILLS", _kills.ToString()),
                new KeyValuePair<string, string>(IsRussianUi ? "Золото получено" : "Gold earned", _runGoldEarned.ToString()) };
            if (_regionStabilized)
            {
                // App.tsx uses the default Basin persistence accessors for these result rows.
                rows.Add(new KeyValuePair<string, string>(IsRussianUi ? "ИСПЫТАНИЯ" : "CHALLENGES", GetCompletedRegionChallenges(EchoRegionId.ResonanceBasin).Length + "/3"));
                rows.Add(new KeyValuePair<string, string>(IsRussianUi ? "БЕСКОНЕЧНЫЙ" : "ENDLESS", IsRegionEndlessUnlocked(EchoRegionId.ResonanceBasin) ? IsRussianUi ? "ОТКРЫТ" : "UNLOCKED" : IsRussianUi ? "ЗАКРЫТ" : "LOCKED"));
                rows.Add(new KeyValuePair<string, string>(IsRussianUi ? "МАСТЕРСТВО" : "MASTERY", _regionMasteryChallengesCompleted + "/3"));
            }
            if (_sourceResultRank > 0 && _sourceResultRank <= 10) rows.Add(new KeyValuePair<string, string>(IsRussianUi ? "Место" : "Rank", "#" + _sourceResultRank));
            var width = Mathf.Min(384f, _uiWidth - 48f); var height = 155f + rows.Count * 28f + (_sourceResultNewRecord ? 42f : 0f);
            var x = (_uiWidth - width) * .5f; var y = Mathf.Max(12f, (_uiHeight - height) * .5f);
            SourceUiLabel(new Rect(x, y, width, 66f), _regionStabilized ? IsRussianUi ? "РЕГИОН СТАБИЛИЗИРОВАН" : "REGION STABILIZED" : IsRussianUi ? "Игра окончена" : "Game Over", 30, _regionStabilized ? "#55e6c1" : "#ff4d5d", true, true);
            y += 74f;
            if (_sourceResultNewRecord) { SourceUiLabel(new Rect(x, y, width, 34f), IsRussianUi ? "Новый рекорд!" : "New Record!", 24, "#ffb84d", true, true); y += 42f; }
            SourceUiRect(new Rect(x, y, width, 28f * rows.Count + 36f), new Color32(13, 23, 38, 255));
            foreach (var row in rows)
            {
                SourceUiLabel(new Rect(x + 24f, y + 18f, width * .65f - 24f, 26f), row.Key, 13, "#7f9bb8");
                var style = new GUIStyle(_label) { alignment = TextAnchor.MiddleRight, fontSize = 13 };
                GUI.Label(new Rect(x + width * .65f, y + 18f, width * .35f - 24f, 26f), row.Value, style); y += 28f;
            }
            y += 60f;
            if (SourceUiButton(new Rect(x, y, width, 48f), IsRussianUi ? "Вернуться в меню" : "Return to menu", 13, new Color(.22f, .85f, 1f, .2f), "#dcecff")) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
