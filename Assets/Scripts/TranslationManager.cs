using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LanguageData
{
    public string language;
    public string displayName;
    public TranslationEntry[] translations;
}

[Serializable]
public class TranslationEntry
{
    public string key;
    public string value;
}

public class TranslationManager : MonoBehaviour
{
    public static TranslationManager Instance { get; private set; }

    private const string DefaultLanguage = "en";
    private const string LanguagePrefsKey = "SelectedLanguage";

    private Dictionary<string, LanguageData> languages =
        new Dictionary<string, LanguageData>();

    private LanguageData currentLanguage;
    private LanguageData fallbackLanguage;

    public string CurrentLanguage =>
        currentLanguage != null ? currentLanguage.language : DefaultLanguage;

    public string CurrentLanguageName =>
        currentLanguage != null ? currentLanguage.displayName : DefaultLanguage;

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

        LoadLanguages();
        SelectStartupLanguage();
    }

    private void LoadLanguages()
    {
        languages.Clear();

        TextAsset[] files =
            Resources.LoadAll<TextAsset>("Languages");

        foreach (TextAsset file in files)
        {
            try
            {
                LanguageData data =
                    JsonUtility.FromJson<LanguageData>(file.text);

                if (data == null ||
                    string.IsNullOrWhiteSpace(data.language))
                {
                    Debug.LogWarning(
                        $"Invalid language file: {file.name}");
                    continue;
                }

                data.language = data.language.ToLowerInvariant();

                if (languages.ContainsKey(data.language))
                {
                    Debug.LogWarning(
                        $"Duplicate language: {data.language}");
                    continue;
                }

                languages.Add(data.language, data);

                Debug.Log(
                    $"Loaded language: {data.displayName} ({data.language})");
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Failed to load {file.name}: {exception.Message}");
            }
        }

        if (languages.TryGetValue(DefaultLanguage, out fallbackLanguage))
        {
            Debug.Log("English fallback loaded.");
        }
        else
        {
            Debug.LogError(
                "Default language 'en' was not found!");
        }
    }

    private void SelectStartupLanguage()
    {
        string savedLanguage = PlayerPrefs.GetString(
            LanguagePrefsKey,
            "");

        if (!string.IsNullOrEmpty(savedLanguage) &&
            SetLanguage(savedLanguage))
        {
            return;
        }

        // Try the device language.
        string systemLanguage =
            Application.systemLanguage.ToString().ToLowerInvariant();

        if (SetLanguage(systemLanguage))
        {
            return;
        }

        // Otherwise use English.
        SetLanguage(DefaultLanguage);
    }

    public bool SetLanguage(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return false;

        languageCode = languageCode.ToLowerInvariant();

        if (!languages.TryGetValue(languageCode, out LanguageData data))
        {
            Debug.LogWarning(
                $"Language '{languageCode}' is not available.");

            return false;
        }

        currentLanguage = data;

        PlayerPrefs.SetString(LanguagePrefsKey, languageCode);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke();

        Debug.Log($"Language changed to: {languageCode}");

        return true;
    }

    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "";

        // Current language first.
        string value = FindTranslation(currentLanguage, key);

        if (value != null)
            return value;

        // English fallback.
        value = FindTranslation(fallbackLanguage, key);

        if (value != null)
            return value;

        // Return the key so missing translations are visible.
        Debug.LogWarning($"Missing translation: {key}");

        return $"[{key}]";
    }

    public string GetText(string key, params object[] args)
    {
        string text = GetText(key);

        if (args == null || args.Length == 0)
            return text;

        try
        {
            return string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                text,
                args);
        }
        catch (FormatException)
        {
            Debug.LogWarning($"Invalid format in translation: {key}");
            return text;
        }
    }

    private string FindTranslation(
        LanguageData language,
        string key)
    {
        if (language == null || language.translations == null)
            return null;

        foreach (TranslationEntry entry in language.translations)
        {
            if (entry != null && entry.key == key)
                return entry.value;
        }

        return null;
    }

    public string[] GetLanguageCodes()
    {
        string[] result = new string[languages.Count];

        languages.Keys.CopyTo(result, 0);

        Array.Sort(result);

        return result;
    }

    public string GetLanguageDisplayName(string languageCode)
    {
        if (languages.TryGetValue(
            languageCode.ToLowerInvariant(),
            out LanguageData data))
        {
            return data.displayName;
        }

        return languageCode;
    }
}