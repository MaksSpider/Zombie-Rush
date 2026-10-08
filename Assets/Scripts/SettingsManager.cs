using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieRush.Framework;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    Dictionary<string, float> playerSettings = new()
    {
        ["display"] = 0f,
        ["volume"] = 1f
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
        Dictionary<string, float> settings = JsonFile.Load<Dictionary<string, float>>("settings");

        if (settings != null)
        {
            playerSettings = settings;
        }

        foreach ((string name, float value) in playerSettings)
        {
            HandleSettingChange(name, value);
        }

        JsonFile.Save("settings", "", playerSettings);
    }

    private void HandleSettingChange(string name, float value)
    {
        switch (name)
        {
            case "volume":
                AudioListener.volume = Mathf.Clamp01(value);
                break;

            case "display":
                if (value == 0f)
                {
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                }
                else if (value == 1f)
                {
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                }
                break;
        }
    }

    public void SetSetting(string name, float value)
    {
        if (!playerSettings.ContainsKey(name)) return;

        playerSettings[name] = value;
        JsonFile.Save("settings", "", playerSettings);

        HandleSettingChange(name, value);
    }

    public float GetSetting(string name)
    {
        if (!playerSettings.ContainsKey(name)) return 0f;

        return playerSettings[name];
    }
}
