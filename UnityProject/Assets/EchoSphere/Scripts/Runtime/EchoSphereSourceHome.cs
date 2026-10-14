using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private Texture2D _sourceMenuCoreTexture;
        private void SourceUiRect(Rect rect, Color color)
        { var previous = GUI.color; GUI.color = color; GUI.DrawTexture(rect, _uiAccentTexture); GUI.color = previous; }
        private void SourceUiLabel(Rect rect, string text, int size, string color, bool center = false, bool bold = false)
        {
            ColorUtility.TryParseHtmlString(color, out var tint);
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, wordWrap = true, alignment = center ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft, fontStyle = bold ? FontStyle.Bold : FontStyle.Normal };
            style.normal.textColor = tint; GUI.Label(rect, text, style);
        }
        private bool SourceUiButton(Rect rect, string text, int size, Color fill, string color)
        {
            SourceUiRect(rect, fill);
            var style = new GUIStyle(GUI.skin.button) { fontSize = size, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            style.normal.background = style.hover.background = style.active.background = style.focused.background = null;
            ColorUtility.TryParseHtmlString(color, out var tint);
            style.normal.textColor = style.hover.textColor = style.active.textColor = tint;
            return SourceGuiButton(rect, new GUIContent(text), style);
        }
        private void DrawSourceMainHome()
        {
            // Active source: App.tsx Menu + final .es-main-menu-legacy CSS overrides.
            SourceUiRect(new Rect(0, 0, _uiWidth, _uiHeight), new Color(2f / 255f, 5f / 255f, 13f / 255f));
            for (var x = 0f; x < _uiWidth; x += 42f) SourceUiRect(new Rect(x, 0f, 1f, _uiHeight), new Color(.31f, .69f, .88f, .012f));
            for (var y = 0f; y < _uiHeight; y += 42f) SourceUiRect(new Rect(0f, y, _uiWidth, 1f), new Color(.31f, .69f, .88f, .012f));
            SourceUiLabel(new Rect(15f, 12f, _uiWidth - 30f, 26f), "ECHO SPHERE", 20, "#e7f4ff", false, true);
            SourceUiLabel(new Rect(15f, 37f, _uiWidth - 30f, 13f), IsRussianUi ? "СИСТЕМА ВЫЖИВАНИЯ РЕЗОНАНСА" : "RESONANCE SURVIVAL SYSTEM", 7, "#5d8da8");
            SourceUiRect(new Rect(0f, 57f, _uiWidth, 1f), new Color(.33f, .72f, .91f, .10f));
            var compact = _uiHeight < 700f;
            var narrowPhone = _uiWidth <= 650f;
            var navHeight = narrowPhone ? 104f : 58f;
            var save = LoadRunSnapshot();
            // Keep the central core and its menu controls inside the usable viewport on
            // short phones. The source menu uses a two-row, three-column bottom navigation
            // on narrow screens, so reserve its real height before sizing the central art.
            var reservedHeight = 58f + navHeight + (compact ? 166f : 209f) + (save != null ? 50f : 0f) + 100f;
            var availableArtHeight = Mathf.Max(96f, _uiHeight - reservedHeight);
            var artSize = compact
                ? Mathf.Min(155f, _uiHeight * .35f, availableArtHeight)
                : Mathf.Min(narrowPhone ? 210f : 270f, _uiWidth * .48f, availableArtHeight);
            var totalHeight = artSize + (compact ? 166f : 209f) + (save != null ? 50f : 0f);
            var top = 58f + Mathf.Max(8f, (_uiHeight - 118f - totalHeight) * .5f);
            var center = _uiWidth * .5f;
            SourceUiLabel(new Rect(center - 180f, top, 360f, 16f), "ECHO CORE // " + (IsRussianUi ? "ГОТОВ" : "READY"), 8, "#5d8da8", true, true);
            var art = new Rect(center - artSize * .5f, top + 19f, artSize, artSize);
            DrawSourceMenuCore(art);
            var map = new Rect(art.x, art.y, art.width, art.height + 44f);
            if (SourceUiButton(map, "", 1, Color.clear, "#e7f4ff")) _regionSelect = true;
            var yNext = art.yMax;
            SourceUiLabel(new Rect(center - 180f, yNext, 360f, 26f), IsRussianUi ? "РЕГИОНЫ ЭХА" : "ECHO REGIONS", 19, "#e7f4ff", true, true);
            SourceUiLabel(new Rect(center - 180f, yNext + 27f, 360f, 13f), IsRussianUi ? "ВЫБРАТЬ ЗАБЕГ" : "SELECT RUN", 7, "#5e849a", true);
            yNext += 44f;
            if (!compact)
            {
                SourceUiLabel(new Rect(center - 165f, yNext, 330f, 32f), IsRussianUi ? "Выбери область, стабилизируй её, открой испытания и доберись до Бесконечности." : "Choose a region, stabilize it, unlock challenges and reach Endless.", 9, "#63849c", true);
                yNext += 38f;
            }
            var width = Mathf.Min(420f, _uiWidth * .88f);
            var loadout = new Rect(center - width * .5f, yNext + (compact ? 5f : 11f), width, 66f);
            SourceUiRect(loadout, new Color(4f / 255f, 11f / 255f, 19f / 255f, .7f));
            CharacterRules.TryParseSourceId(PlayerPrefs.GetString(CharacterPreferenceKey, "spherist"), out var id);
            var character = CharacterRules.Get(id);
            if (SourceUiButton(new Rect(loadout.x + 7f, loadout.y + 7f, loadout.width - 14f, 29f), "", 10, Color.clear, "#e8f5ff")) _mainMenuPage = MainMenuPage.Characters;
            SourceUiLabel(new Rect(loadout.x + 14f, loadout.y + 7f, 24f, 25f), "✦", 18, character.ColorHex, true);
            SourceUiLabel(new Rect(loadout.x + 45f, loadout.y + 6f, width - 60f, 16f), IsRussianUi ? character.NameRu : character.NameEn, 11, "#dcefff", false, true);
            SourceUiLabel(new Rect(loadout.x + 45f, loadout.y + 22f, width - 60f, 12f), IsRussianUi ? character.RoleRu : character.RoleEn, 7, "#68879d");
            SourceUiLabel(new Rect(loadout.x + 12f, loadout.y + 41f, 80f, 17f), IsRussianUi ? "СЛОЖНОСТЬ" : "DIFFICULTY", 7, "#63849c");
            var difficultyIds = new[] { "easy", "normal", "hard", "nightmare" };
            var difficultyNames = IsRussianUi ? new[] { "Лёгкая", "Обычная", "Сложная", "Кошмар" } : new[] { "Easy", "Normal", "Hard", "Nightmare" };
            var currentDifficulty = PlayerPrefs.GetString(DifficultyPreferenceKey, "normal");
            var buttonWidth = (width - 102f) / 4f;
            for (var i = 0; i < difficultyIds.Length; i++)
                if (SourceUiButton(new Rect(loadout.x + 94f + i * buttonWidth, loadout.y + 40f, buttonWidth - 3f, 20f), difficultyNames[i], 7,
                    currentDifficulty == difficultyIds[i] ? new Color(.13f, .42f, .53f, .25f) : new Color(.03f, .07f, .12f, .76f), currentDifficulty == difficultyIds[i] ? "#e8fbff" : "#68879d"))
                { PlayerPrefs.SetString(DifficultyPreferenceKey, difficultyIds[i]); PlayerPrefs.Save(); }
            var nameRect = new Rect(loadout.x, loadout.yMax + 6f, width, 27f);
            SourceUiLabel(new Rect(nameRect.x + 5f, nameRect.y, 24f, nameRect.height), "ID", 9, "#63849c");
            var name = GUI.TextField(new Rect(nameRect.x + 32f, nameRect.y, width - 32f, nameRect.height), _playerName, 16, new GUIStyle(_button) { fontSize = 10 });
            if (name != _playerName) { _playerName = name; PlayerPrefs.SetString(NamePreferenceKey, name); PlayerPrefs.Save(); }
            if (save != null && SourceUiButton(new Rect(nameRect.x, nameRect.yMax + 8f, width, 42f),
                (IsRussianUi ? "ПРОДОЛЖИТЬ ЗАБЕГ\nСохранено на " : "CONTINUE RUN\nSaved at ") + FormatTime(save.time), 10, new Color(.14f, .44f, .55f, .28f), "#dffaff")) ContinueSavedRun();
            var labels = IsRussianUi ? new[] { "ПЕРСОНАЖИ", "МАГАЗИН", "РЕКОРДЫ", "ДОСТИЖЕНИЯ", "АРХИВ ЭХА", "НАСТРОЙКИ" } : new[] { "CHARACTERS", "SHOP", "LEADERBOARD", "ACHIEVEMENTS", "ECHO ARCHIVE", "SETTINGS" };
            var pages = new[] { MainMenuPage.Characters, MainMenuPage.Shop, MainMenuPage.Leaderboard, MainMenuPage.Achievements, MainMenuPage.Knowledge, MainMenuPage.Settings };
            var navTop = _uiHeight - navHeight;
            SourceUiRect(new Rect(0f, navTop, _uiWidth, navHeight), new Color(2f / 255f, 7f / 255f, 13f / 255f, .88f));
            var navColumns = narrowPhone ? 3 : 6;
            var navRows = Mathf.CeilToInt(pages.Length / (float)navColumns);
            var navGap = 5f;
            var navWidth = (_uiWidth - 20f - (navColumns - 1) * navGap) / navColumns;
            var navButtonHeight = narrowPhone ? 42f : 42f;
            var navVerticalGap = narrowPhone ? 4f : 0f;
            var navContentHeight = navRows * navButtonHeight + (navRows - 1) * navVerticalGap;
            var navStartY = navTop + Mathf.Max(4f, (navHeight - navContentHeight) * .5f);
            for (var i = 0; i < pages.Length; i++)
            {
                var column = i % navColumns;
                var row = i / navColumns;
                var rect = new Rect(10f + column * (navWidth + navGap), navStartY + row * (navButtonHeight + navVerticalGap), navWidth, navButtonHeight);
                if (SourceUiButton(rect, "", 7, Color.clear, "#66869d")) _mainMenuPage = pages[i];
                var iconColor = new Color(102f / 255f, 134f / 255f, 157f / 255f, 1f);
                DrawSourceMenuNavIcon(new Rect(rect.x + 5f, rect.y + 14f, 14f, 14f), i, iconColor);
                SourceUiLabel(new Rect(rect.x + 22f, rect.y + 1f, rect.width - 25f, rect.height - 2f), labels[i], 7, "#66869d", true);
            }
        }
        // Matches the Lucide icon identities used by src/App.tsx. Presentation only.
        private void DrawSourceMenuNavIcon(Rect rect, int index, Color color)
        {
            switch (index)
            {
                case 0: // UserRound
                    SourceUiNavCircle(rect, new Vector2(.5f, .29f), .17f, color, 1.25f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.18f,.88f), new Vector2(.18f,.72f), new Vector2(.29f,.57f), new Vector2(.5f,.52f), new Vector2(.71f,.57f), new Vector2(.82f,.72f), new Vector2(.82f,.88f) }, color, 1.25f);
                    break;
                case 1: // Store
                    SourceUiNavStroke(rect, new[] { new Vector2(.10f,.37f), new Vector2(.22f,.12f), new Vector2(.78f,.12f), new Vector2(.90f,.37f) }, color, 1.25f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.14f,.38f), new Vector2(.14f,.88f), new Vector2(.86f,.88f), new Vector2(.86f,.38f) }, color, 1.25f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.10f,.37f), new Vector2(.90f,.37f) }, color, 1.25f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.38f,.88f), new Vector2(.38f,.57f), new Vector2(.62f,.57f), new Vector2(.62f,.88f) }, color, 1.1f);
                    break;
                case 2: // Trophy
                    SourceUiNavStroke(rect, new[] { new Vector2(.29f,.12f), new Vector2(.71f,.12f), new Vector2(.68f,.49f), new Vector2(.59f,.61f), new Vector2(.41f,.61f), new Vector2(.32f,.49f), new Vector2(.29f,.12f) }, color, 1.25f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.29f,.22f), new Vector2(.10f,.22f), new Vector2(.12f,.43f), new Vector2(.32f,.50f) }, color, 1.1f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.71f,.22f), new Vector2(.90f,.22f), new Vector2(.88f,.43f), new Vector2(.68f,.50f) }, color, 1.1f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.50f,.61f), new Vector2(.50f,.82f), new Vector2(.32f,.88f), new Vector2(.68f,.88f) }, color, 1.25f);
                    break;
                case 3: // Award
                    SourceUiNavStroke(rect, new[] { new Vector2(.50f,.08f), new Vector2(.63f,.35f), new Vector2(.91f,.39f), new Vector2(.70f,.59f), new Vector2(.75f,.88f), new Vector2(.50f,.74f), new Vector2(.25f,.88f), new Vector2(.30f,.59f), new Vector2(.09f,.39f), new Vector2(.37f,.35f), new Vector2(.50f,.08f) }, color, 1.15f);
                    break;
                case 4: // BookOpen
                    SourceUiNavStroke(rect, new[] { new Vector2(.49f,.20f), new Vector2(.39f,.12f), new Vector2(.12f,.12f), new Vector2(.12f,.82f), new Vector2(.39f,.82f), new Vector2(.49f,.89f), new Vector2(.49f,.20f) }, color, 1.15f);
                    SourceUiNavStroke(rect, new[] { new Vector2(.51f,.20f), new Vector2(.61f,.12f), new Vector2(.88f,.12f), new Vector2(.88f,.82f), new Vector2(.61f,.82f), new Vector2(.51f,.89f), new Vector2(.51f,.20f) }, color, 1.15f);
                    break;
                case 5: // Settings
                    SourceUiNavCircle(rect, new Vector2(.5f,.5f), .30f, color, 1.15f);
                    SourceUiNavCircle(rect, new Vector2(.5f,.5f), .10f, color, 1.15f);
                    for (var tooth = 0; tooth < 8; tooth++)
                    {
                        var angle = tooth * Mathf.PI / 4f;
                        var inner = new Vector2(.5f + Mathf.Cos(angle) * .31f, .5f + Mathf.Sin(angle) * .31f);
                        var outer = new Vector2(.5f + Mathf.Cos(angle) * .45f, .5f + Mathf.Sin(angle) * .45f);
                        SourceUiNavStroke(rect, new[] { inner, outer }, color, 1.15f);
                    }
                    break;
            }
        }
        private void SourceUiNavCircle(Rect rect, Vector2 center, float radius, Color color, float width)
        {
            var previous = new Vector2();
            for (var i = 0; i <= 20; i++)
            {
                var angle = i * Mathf.PI * 2f / 20f;
                var point = new Vector2(center.x + Mathf.Cos(angle) * radius, center.y + Mathf.Sin(angle) * radius);
                if (i > 0) DrawSphereGuiLine(new Vector2(rect.x + previous.x * rect.width, rect.y + previous.y * rect.height), new Vector2(rect.x + point.x * rect.width, rect.y + point.y * rect.height), color, width);
                previous = point;
            }
        }
        private void SourceUiNavStroke(Rect rect, Vector2[] points, Color color, float width)
        {
            for (var i = 1; i < points.Length; i++)
                DrawSphereGuiLine(new Vector2(rect.x + points[i - 1].x * rect.width, rect.y + points[i - 1].y * rect.height), new Vector2(rect.x + points[i].x * rect.width, rect.y + points[i].y * rect.height), color, width);
        }
        private void DrawSourceMenuCore(Rect art)
        {
            if (_sourceMenuCoreTexture == null)
            {
                const int size = 128;
                _sourceMenuCoreTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var stops = new[] { 0f, .05f, .13f, .29f, .53f, .73f, 1f };
                var hex = new[] { "#ffffff", "#ffffff", "#c7f8ff", "#5bdcff", "#26558c", "#070d19", "#010309" };
                var colors = new Color[hex.Length]; for (var i = 0; i < hex.Length; i++) ColorUtility.TryParseHtmlString(hex[i], out colors[i]);
                for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
                {
                    var point = new Vector2(x / (size - 1f), 1f - y / (size - 1f));
                    var r = Vector2.Distance(point, new Vector2(.33f, .27f)); var segment = 1;
                    while (segment < stops.Length - 1 && r > stops[segment]) segment++;
                    var color = Color.Lerp(colors[segment - 1], colors[segment], Mathf.InverseLerp(stops[segment - 1], stops[segment], r));
                    color.a = Mathf.Clamp01((.5f - Vector2.Distance(point, new Vector2(.5f, .5f))) * size);
                    _sourceMenuCoreTexture.SetPixel(x, y, color);
                }
                _sourceMenuCoreTexture.Apply();
            }
            var previous = GUI.color; GUI.color = Color.white;
            GUI.DrawTexture(new Rect(art.x + art.width * .28f, art.y + art.height * .28f, art.width * .44f, art.height * .44f), _sourceMenuCoreTexture);
            var radii = new[] { .41f, .30f, .19f, .35f, .11f };
            var rotations = new[] { 26f, -50f, -34f, -16f, -24f };
            for (var ring = 0; ring < radii.Length; ring++)
            {
                var points = new Vector2[97]; var angle = rotations[ring] * Mathf.Deg2Rad;
                for (var i = 0; i < points.Length; i++)
                {
                    var a = i * Mathf.PI * 2f / 96f;
                    var x = Mathf.Cos(a) * radii[ring] * art.width; var y = Mathf.Sin(a) * radii[ring] * art.height * .38f;
                    points[i] = art.center + new Vector2(x * Mathf.Cos(angle) - y * Mathf.Sin(angle), x * Mathf.Sin(angle) + y * Mathf.Cos(angle));
                    if (i > 0) DrawSphereGuiLine(points[i - 1], points[i], ring == 2 ? new Color(1f, .72f, .35f, .18f) : new Color(.39f, .85f, 1f, .22f), 1f);
                }
            }
            GUI.color = previous;
        }
    }
}
