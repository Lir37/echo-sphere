"""Audit exported, resolved player text after the two Unity copy checks."""
import json
import pathlib
import re

root = pathlib.Path(__file__).resolve().parents[1]
folder = root / "UnityProject/Library/player-copy"
copy = json.loads((root / "UnityProject/Assets/Resources/EchoSphere/PlayerCopy.json").read_text(encoding="utf-8"))
issues = []
checks = 0
for language in ("Ru", "En"):
    entries = copy[language]
    assert len({r["Source"] for r in entries}) == len(entries), "Duplicate revision"
    for row in entries:
        assert sorted(re.findall(r"\{[^}]+\}", row["Source"])) == sorted(re.findall(r"\{[^}]+\}", row["Text"])), "Changed format placeholders"
        checks += 1
assert len(copy["Abilities"]) == len({r["Id"] for r in copy["Abilities"]}) == 30


def check(path, language, text):
    global checks
    checks += 1
    cleaned = re.sub(r"\{[^}]+\}|ECHO SPHERE|\b[IVX]+\b", "", text)
    bad = bool(re.search(r"\b(RNG|runtime|native|payload|cadence|ticks?|POI|debug|undefined)\b|\[object Object\]", text, re.I))
    bad |= bool(re.search(r"[a-zA-Z]{2}", cleaned)) if language == "ru" else bool(re.search(r"[а-яА-ЯёЁ]", text))
    if bad:
        issues.append(dict(context=path, language=language, text=text))


for row in json.loads((folder / "resolved.json").read_text(encoding="utf-8")):
    check(row["Context"], row["Language"], row["Text"])

# Non-visible identifiers, colors, CSS tones and user names are not translations.
metadata = {"Id", "Key", "Category", "Section", "Query", "Group", "Color", "ElementColor", "Portrait", "Difficulty", "CharacterId", "Tone"}


def walk(value, path, language):
    if isinstance(value, dict):
        for key, child in value.items():
            if isinstance(child, str) and key not in metadata:
                check(path + "/" + key, language, child)
            elif isinstance(child, (list, dict)):
                walk(child, path + "/" + key, language)
    elif isinstance(value, list):
        for i, child in enumerate(value):
            if isinstance(child, str):
                check(path + "/" + str(i), language, child)
            else:
                walk(child, path + "/" + str(i), language)


states = json.loads((folder / "runtime-states.json").read_text(encoding="utf-8"))
for row in states:
    walk(row["state"], row["screen"], row["language"])
result = dict(result="FAIL" if issues else "PASS", checks=checks, runtimeStates=len(states), revisionsRu=len(copy["Ru"]), revisionsEn=len(copy["En"]), abilitySummaries=len(copy["Abilities"]), issues=issues)
(folder / "audit.json").write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
print(json.dumps(result, ensure_ascii=True))
raise SystemExit(bool(issues))
