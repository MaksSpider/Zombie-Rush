using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieRush.Framework;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    private TranslationManager _translationManager => TranslationManager.Instance;

    private Dictionary<string, object> playerSettings = new()
    {
        ["display"] = 0f,
        ["volume"] = 1f,
        ["language"] = "en",
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Dictionary<string, object> settings =
            JsonFile.Load<Dictionary<string, object>>("settings");

        if (settings != null)
        {
            playerSettings = settings;
        }

        foreach (var setting in playerSettings)
        {
            HandleSettingChange(setting.Key, setting.Value);
        }

        JsonFile.Save("settings", "", playerSettings);
    }

    private void HandleSettingChange(string name, object value)
    {
        switch (name)
        {
            case "volume":
                if (TryGetFloat(value, out float volume))
                {
                    AudioListener.volume = Mathf.Clamp01(volume);
                }
                break;

            case "display":
                if (TryGetFloat(value, out float display))
                {
                    if (display == 0f)
                    {
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    }
                    else if (display == 1f)
                    {
                        Screen.fullScreenMode = FullScreenMode.Windowed;
                    }
                }
                break;

            case "language":
                if (value is string language)
                {
                    //_translationManager.SetLanguage(language);
                }
                break;
        }
    }

    private bool TryGetFloat(object value, out float result)
    {
        try
        {
            result = Convert.ToSingle(value);
            return true;
        }
        catch
        {
            result = 0f;
            return false;
        }
    }

    public void SetSetting(string name, object value)
    {
        if (!playerSettings.ContainsKey(name)) return;

        playerSettings[name] = value;

        JsonFile.Save("settings", "", playerSettings);

        HandleSettingChange(name, value);
    }

    public object GetSetting(string name)
    {
        if (!playerSettings.ContainsKey(name))
            return null;

        return playerSettings[name];
    }

    public T GetSetting<T>(string name, T defaultValue = default)
    {
        if (!playerSettings.TryGetValue(name, out object value))
            return defaultValue;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
}
