# ECHO SPHERE — HUD, компактное меню, снаряды и мутации

2026-10-29. База caaedaf6, unity-migration. Unity6000.6.1f1.

## Изменения

- NativeMenuBrushCache отпускает заменённые skins спустя3кадра. Раньше
  NativeHudSurface.Prepare сразу Release/Destroy старой текстуры, хотя UI Toolkit
  ещё мог хранить команды/ссылки на неё. NativeMenuBrush восстанавливает потерянный
  GPU-буфер; TextureVersion заставляет все Image-потребители повторно привязать его.
  Отдельно проверены задержка освобождения, очистка очереди и forced target loss.
  Это исправление подтверждённой уязвимости владения ресурсами; связь с конкретным
  белым HUD пользователя пока не доказана. Эффекты/тени/стекло не отключались.
- Characters/Shop/Leaderboard/Achievements/Archive/Settings/Regions: scale=DPR×0.86.
  Главное меню и бой/HUD/пользовательская настройка масштаба боя прежние.
- NativeWorldProjectiles: retained spine/fins/wake для6видов вражеских снарядов;
  NativeWorldSphereMutationDetails:5головок/направляющие/светлая сердцевина для
  существующих снарядов сфер. Сохранены branch FX, число/движение/скорость/урон,
  коллизии,статусы и RNG. Новые детали не добавляют Gaussian blur passes.
- Кометный след: одна retained mesh-лента,до38вершин,топология обновляется только
  при изменении числа samples;позиции/цвета переиспользуют буферы.18samples/0.38s,
  alpha≤0.14,мягкие края на GPU,цвет персонажа;без ParticleSystem/blur/RNG/save.
  Корпус не разворачивается от движения;телепорт/смена персонажа/гибель сбрасывают след.
- Призма: удалено перекрашивание основного ядра/орбит в цвет статуса Спектра.
  Основной розовый сохранён;отдельные elemental accents/effects и механики прежние.
  Это прямое уточнение пользователя,более не требовать старую tint-parity в preflight.

## Проверки

- Actual UI Toolkit GPU:46checks,10состояний персонажей,движущийся radar/HP,
  принудительная потеря правой панели/HP текстур и восстановление — PASS.
- Actual runtime menu panels:32checks,8страниц — PASS;главная scale не изменилась.
- Projectile GPU:92checks,6enemy kinds/5friendly families,56retained detail meshes — PASS.
- Комета/направления:1700checks,10персонажей×4направления,цвет/затухание/teleport — PASS.
- Призма:52GPUcases,base/IV/VII × null/fire/freeze/poison;body+base orbits одинаковы.
- VII proposals:581checks,10семейств/30IVветок/90VIIфиналов;Core и реальные
  BuildNativeUpgradeInputs соответствуют выбранному родителю и текстам — PASS.
  Привязка уже была корректной;правила прогрессии не менялись.
- Export source catalog305точных строк,preflight и5C#profiles — PASS.
- После чистого Play Console0errors/0warnings;Editor prefs10keys restored/verified.
  Промежуточный live script reload при разработке сбросил managed menu composition
  и вызвал Editor-only NullReference spam;Play перезапущен до финальных проверок.
  Это не выдаётся за воспроизведение белого Android HUD.

## Телефон до новой сборки

На старом39af96d0 пользователь сообщил полностью белый правый HUD и белую левую
HP-полосу у Охотника. После холодного запуска в нашей записи с реальным joystick
справа (0:29–0:37,HP94→88) HUD оставался цветным. Есть видео/снимки в external
user-feedback. Пользователь затем тоже сообщил,что белого HUD сейчас нет;
на телефоне в тот момент ещё старый APK,не связывать это наблюдение с новым кодом.
Повторная Android-проверка новой сборки остаётся обязательной. Профиль не сбрасывался;
короткий обычный тест продвинул сохранённый забег,никаких synthetic unlocks не было.
APK/установка/свежий backup и CI фиксируются в общем журнале после выполнения.
