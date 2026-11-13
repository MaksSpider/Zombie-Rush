using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    private TranslationManager _translationManager => TranslationManager.Instance;
    private SettingsManager _settingsManager => SettingsManager.Instance;

    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Toggle _windowedToggle;
    [SerializeField] private TMP_Dropdown _languageDropdown;

    [SerializeField] private Scrollbar _volumeScrollbar;

    void Start()
    {
        if (_settingsManager.GetSetting("volume") is float volume)
        {
            _volumeScrollbar.value = volume;
        }

        List<string> languages = _translationManager.GetLanguagesList();
        List<TMP_Dropdown.OptionData> languageOptions = new();
        int languageDropdownValue = 0;

        for(int i = 0; i < languages.Count; i++)
        {
            if (languages[i] == _translationManager.currentLanguage) languageDropdownValue = i;
            languageOptions.Add(new TMP_Dropdown.OptionData(_translationManager.GetText("language", languages[i]), null, new Color(1, 1, 1)));
        }

        _languageDropdown.ClearOptions();
        _languageDropdown.AddOptions(languageOptions);
        _languageDropdown.value = languageDropdownValue;
        _languageDropdown.RefreshShownValue();

        _languageDropdown.onValueChanged.AddListener(value =>
        {
            _settingsManager.SetSetting("language", languages[value]);
        });

        _volumeScrollbar.onValueChanged.AddListener(value =>
        {
            _settingsManager.SetSetting("volume", value);
        });

        if (_settingsManager.GetSetting("display") is float display)
        {
            ChangeScreenType(display);
        }

        _fullscreenToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn) ChangeScreenType(0f);
            else ChangeScreenType(1f);
        });

        _windowedToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn) ChangeScreenType(1f);
            else ChangeScreenType(0f);
        });
    }

    private void ChangeScreenType(float option)
    {
        switch(option)
        {
            case 0f:
                _fullscreenToggle.isOn = true;
                _windowedToggle.isOn = false;
                break;
            case 1f:
                _fullscreenToggle.isOn = false;
                _windowedToggle.isOn = true;
                break;
            default: 
                break;
        }

        _settingsManager.SetSetting("display", option);
    }
}
