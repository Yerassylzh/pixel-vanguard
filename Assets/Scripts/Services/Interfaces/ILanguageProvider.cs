namespace PixelVanguard.Services
{
    /// <summary>
    /// Interface for platform-specific language detection and switching.
    /// Implemented by YandexLanguageProvider (WebGL) and DefaultLanguageProvider (Android/Editor).
    /// </summary>
    public interface ILanguageProvider
    {
        /// <summary>
        /// Initialize platform-specific language hooks (e.g., Yandex events).
        /// </summary>
        void Initialize();

        /// <summary>
        /// Get the current language code from platform.
        /// Returns "en" or "ru".
        /// </summary>
        string GetCurrentLanguage();

        /// <summary>
        /// Get list of supported language codes.
        /// Returns ["en", "ru"].
        /// </summary>
        string[] GetSupportedLanguages();

        /// <summary>
        /// Switch to a different language (if platform supports manual switching).
        /// </summary>
        void SwitchLanguage(string languageCode);
    }
}
