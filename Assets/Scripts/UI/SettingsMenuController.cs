using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SettingsMenuController : MonoBehaviour
{
    private SettingsManager _settingsManager => SettingsManager.Instance;

    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Toggle _windowedToggle;

    [SerializeField] private Scrollbar _volumeScrollbar;

    void Start()
    {
        if (_settingsManager.GetSetting("volume") is float volume)
        {
            _volumeScrollbar.value = volume;
        }

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
