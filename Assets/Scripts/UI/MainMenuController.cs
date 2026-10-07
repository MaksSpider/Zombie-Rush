using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private GameSession _gameSession => GameSession.Instance;
    private AudioManager _audioManager => AudioManager.Instance;

    // mainmenu
    [SerializeField] private Button _openGameSetupButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    // game setup menu

    [SerializeField] private GameObject SetupMenu;
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_InputField _seedInputField;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _closeButton;

    void Start()
    {
        _openGameSetupButton.onClick.AddListener(OpenSetupMenu);
        _settingsButton.onClick.AddListener(OpenSettings);
        _quitButton.onClick.AddListener(Quit);

        _startButton.onClick.AddListener(StartGame);
        _closeButton.onClick.AddListener(CloseSetupMenu);

        SetupMenu.SetActive(false);

        _audioManager.PlayMusic("mainmenu");
    }

    private void OpenSetupMenu()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        _seedInputField.text = seed.ToString();

        SetupMenu.SetActive(true);
    }

    private void CloseSetupMenu()
    {
        SetupMenu.SetActive(false);

        _nicknameInputField.text = string.Empty;
        _seedInputField.text = string.Empty;
    }

    private void StartGame()
    {
        if (string.IsNullOrWhiteSpace(_nicknameInputField.text)) return;
        if (string.IsNullOrWhiteSpace(_seedInputField.text)) return;

        _gameSession.Setup(_nicknameInputField.text, int.Parse(_seedInputField.text));

        SetupMenu.SetActive(false);
        SceneManager.LoadScene("Game");
    }

    // TODO: implement settings
    private void OpenSettings()
    {

    }

    private void Quit()
    {
        Application.Quit();
    }
}
