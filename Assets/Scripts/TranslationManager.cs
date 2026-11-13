using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LanguageInfo
{
    public string language;
    public string displayName;
}
public class TranslationFile
{
    public Dictionary<string, string> translations;
}

public class TranslationManager : MonoBehaviour
{
    public static TranslationManager Instance { get; private set; }

    private SettingsManager _settingsManager => SettingsManager.Instance;

    [SerializeField] private string _defaultLanguage = "en";

    private readonly Dictionary<string, Dictionary<string, string>> _languages = new();

    private string _currentLanguage;

    private bool _initialized;

    public string currentLanguage => !string.IsNullOrEmpty(_currentLanguage) ? _currentLanguage : _defaultLanguage;

    public event Action OnLanguageChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _defaultLanguage = _defaultLanguage.ToLowerInvariant();

        if (_settingsManager.GetSetting("language") is string language)
        {
            _currentLanguage = language;
        }

        LoadLanguages();
    }

    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        SetLanguage(_defaultLanguage);
    }

    private void LoadLanguages()
    {
        _languages.Clear();

        TextAsset[] metadataFiles = Resources.LoadAll<TextAsset>("Languages");

        foreach (TextAsset file in metadataFiles)
        {
            if (!file.name.Equals("language", StringComparison.OrdinalIgnoreCase)) continue;

            LanguageInfo info = JsonConvert.DeserializeObject<LanguageInfo>(file.text);
            if (info == null || string.IsNullOrWhiteSpace(info.language)) continue;

            if (_languages.ContainsKey(info.language)) continue;

            LoadLanguage(info.language, info.displayName);
        }

        if (!_languages.ContainsKey(_defaultLanguage))
            Debug.LogError($"Default language '{_defaultLanguage}' was not found!");
    }

    private void LoadLanguage(string code, string displayName)
    {
        Dictionary<string, string> entries = new();
        TextAsset[] files = Resources.LoadAll<TextAsset>($"Languages/{code}");

        foreach (TextAsset file in files)
        {
            if (file.name.Equals("language", StringComparison.OrdinalIgnoreCase)) continue;

            TranslationFile translationFile = JsonConvert.DeserializeObject<TranslationFile>(file.text);

            if (translationFile == null || translationFile.translations == null)
            {
                Debug.LogWarning($"Invalid translation file: {code}/{file.name}.json");
                continue;
            }

            foreach ((string key, string value) in translationFile.translations)
            {
                if (string.IsNullOrEmpty(key)) continue;

                if (entries.ContainsKey(key))
                {
                    Debug.LogWarning($"Duplicate key: {code}/{file.name}/{key}");
                    continue;
                }

                entries.Add(key, value ?? "");
            }

            Debug.Log($"Loaded category: {code}/{file.name} ({entries.Count} translations)");
        }

        entries.Add("language", displayName);
        _languages.Add(code, entries);

        Debug.Log($"Loaded language: {displayName} ({code})");
    }

    public bool SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return false;

        languageCode = languageCode.ToLowerInvariant();

        if (!_languages.ContainsKey(languageCode))
        {
            Debug.LogWarning($"Language '{languageCode}' is not available.");
            return false;
        }

        if (_currentLanguage == languageCode) return true;

        _currentLanguage = languageCode;

        OnLanguageChanged?.Invoke();

        Debug.Log($"Language changed to: {languageCode}");

        return true;
    }

    public string GetSystemLanguageCode()
    {
        return Application.systemLanguage switch
        {
            SystemLanguage.English => "en",
            SystemLanguage.Polish => "pl",
            SystemLanguage.German => "de",
            SystemLanguage.Spanish => "es",
            _ => _defaultLanguage
        };
    }

    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";

        foreach((string k, string v) in _languages[currentLanguage])
        {
            if (k == key) return v;
        }

        foreach ((string k, string v) in _languages[_defaultLanguage])
        {
            if (k == key)
            {
                Debug.LogWarning($"Missing translation: {currentLanguage}/{key}");
                return v;
            }
        }

        Debug.LogWarning($"Missing translation: {currentLanguage}/{key}");
        return $"{currentLanguage}.{key}";
    }

    public string GetText(string key, string language)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(language)) return "";

        foreach ((string k, string v) in _languages[language])
        {
            if (k == key) return v;
        }

        Debug.LogWarning($"Missing translation: {language}/{key}");
        return $"{language}.{key}";
    }

    public List<string> GetLanguagesList()
    {
        return _languages.Keys.ToList();
    }
}