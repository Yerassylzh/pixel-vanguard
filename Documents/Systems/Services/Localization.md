# Localization System

**Location:** `Systems/Services/Localization.md`
**Code Path:** `Assets/Scripts/Core/LocalizationManager.cs`

## 1. Architecture
*   **Static Access:** `LocalizationManager.Get("ui.play")`.
*   **Dynamic Provider:** `ILanguageProvider` (Swaps based on Platform).
*   **Reactive:** Fires `OnLanguageChanged` event; all UI updates instantly.

## 2. Providers
| Platform | Provider | Method |
| :--- | :--- | :--- |
| **Yandex** | `YandexLanguageProvider` | Reads `YG.lang` from browser URL/Settings. |
| **Android** | `DefaultLanguageProvider` | Reads `Application.systemLanguage`. |
| **Editor** | `DefaultLanguageProvider` | Manual toggle for testing. |

## 3. Data Source
*   **Asset:** `Data/Translations.asset`
*   **Structure:** List of keys with EN/RU values.
*   **Fallback:** If RU missing, returns EN. If EN missing, returns Key.
