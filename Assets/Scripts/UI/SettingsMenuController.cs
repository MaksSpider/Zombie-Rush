using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    private SettingsManager _settingsManager => SettingsManager.Instance;

    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Toggle _windowedToggle;

    [SerializeField] private Scrollbar _volumeScrollbar;

    void Start()
    {
        _volumeScrollbar.value = _settingsManager.GetSetting("volume");

        _volumeScrollbar.onValueChanged.AddListener(value =>
        {
            _settingsManager.SetSetting("volume", value);
        });

        ChangeScreenType(_settingsManager.GetSetting("display"));

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
