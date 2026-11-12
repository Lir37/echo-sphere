# ECHO SPHERE · подготовка релиза Android для RuStore

Аудит от 12.11.2026. Исходный HEAD c0c9009b4a613e057a136af7914685046b10ca16. Комиксы отложены до обновления, материалы хранятся в GPT и не входят в Unity. Таймер стабилизации оставлен 1800 секунд.

## Сборка

- Unity 6000.6.1f1, пакет com.lir37.echosphere, IL2CPP, ARM64 и ARMv7, min SDK 26. Релизный метод фиксирует target SDK 36, установленный в локальном Unity SDK. Обычный тестовый метод GitHub Actions остаётся доступным.
- В Unity batch mode вызвать EchoSphereCiBuild.BuildAndroidReleaseApk с параметрами -batchmode -quit -projectPath <UnityProject> -buildTarget Android -executeMethod EchoSphereCiBuild.BuildAndroidReleaseApk. В текущем процессе задать ECHO_ANDROID_KEYSTORE, ECHO_ANDROID_KEY_ALIAS, ECHO_ANDROID_KEYSTORE_PASSWORD, ECHO_ANDROID_KEY_PASSWORD, ECHO_ANDROID_VERSION и ECHO_ANDROID_VERSION_CODE. Путь, ключ и пароли не добавлять в репозиторий. Метод собирает Builds/Android/ECHO-SPHERE-release.apk без Development/AllowDebugging. Перед запуском убрать ECHO_SPHERE_TEST_CONTROLS, если он выставлен в Unity.
- Перед выдачей APK выполнить apksigner verify --print-certs и aapt dump badging. Проверить пакет, сертификат, versionCode выше опубликованного или установленного, SDK, ABI, разрешения и отсутствие application-debuggable. Сверить сертификат с выбранным релизным сертификатом.
- Проверенный локальный тестовый APK ECHO-SPHERE-65a3f28f-regions-test.apk: 0.1.0 (1), min 26, target 36, ARM64/ARMv7, Android Debug. В манифесте INTERNET и служебное разрешение DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION. Итоговый список разрешений релиза сверить по готовому APK.

## Обновление и прогресс

Сначала снять package name, versionCode и отпечаток сертификата с фактически установленного APK. Android сохраняет данные приложения при обновлении с совместимой подписью и тем же package name. Обычная отдельная релизная подпись не обновит установленный APK с Android Debug. Проверку проводить на тестовом устройстве или профиле: установить предыдущую сборку, создать прогресс, затем обновить поверх и проверить персонажей, валюту, регион и продолжение забега. Если подписи несовместимы, сохранить текущую установку и проверить сценарий на двух сборках с одним релизным сертификатом. На момент аудита ADB не видел устройства.

## Открытие регионов

Утверждённая последовательность: стабилизация, три испытания, следующий регион и Endless текущего. CampaignRegions.IsUnlocked проверяет стабилизацию предыдущего региона. EchoSphereRuntime.IsRegionEndlessUnlocked разрешает Endless уже после стабилизации. CompleteCampaignRegionalProgress также записывает флаг Endless. Правило трёх испытаний записывает прохождение, но не ограничивает эти два пути. Изменение требует отдельного согласования, включая судьбу уже открытых регионов.

Источники RuStore:
- https://www.rustore.ru/help/developers/publishing-and-verifying-apps/app-publication
- https://www.rustore.ru/help/developers/publishing-and-verifying-apps/app-publication/apk-signature
