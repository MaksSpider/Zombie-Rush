using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZombieRush.Framework;

public class MainMenuController : MonoBehaviour
{
    private GameSession _gameSession => GameSession.Instance;
    private AudioManager _audioManager => AudioManager.Instance;

    // mainmenu
    [SerializeField] private Button _openGameSetupButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    // game setup menu
    [SerializeField] private GameObject _setupMenu;
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_InputField _seedInputField;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _closeButton;

    // scoreboard
    [SerializeField] private GameObject _scoreboardMenu;
    [SerializeField] private Transform _scoreboardGrid;

    // settings menu
    [SerializeField] private GameObject _settingsMenu;

    private List<PlayerProfile> scoreboard = new();

    private bool _isSettingsOpen = false;

    void Start()
    {
        _openGameSetupButton.onClick.AddListener(OpenSetupMenu);
        _settingsButton.onClick.AddListener(OpenSettings);
        _quitButton.onClick.AddListener(Quit);

        _startButton.onClick.AddListener(StartGame);
        _closeButton.onClick.AddListener(CloseSetupMenu);

        _setupMenu.SetActive(false);
        _settingsMenu.SetActive(false);

        _audioManager.PlayMusic("mainmenu");

        scoreboard = JsonFile.LoadAll<PlayerProfile>("scoreboard").OrderByDescending(x => x.score).ToList();
        DisplayScorebaord();
    }

    private void OpenSetupMenu()
    {
        int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _seedInputField.text = seed.ToString();

        _setupMenu.SetActive(true);
    }

    private void CloseSetupMenu()
    {
        _setupMenu.SetActive(false);

        _nicknameInputField.text = string.Empty;
        _seedInputField.text = string.Empty;
    }

    private void StartGame()
    {
        if (string.IsNullOrWhiteSpace(_nicknameInputField.text)) return;
        if (string.IsNullOrWhiteSpace(_seedInputField.text)) return;

        _gameSession.Setup(_nicknameInputField.text, int.Parse(_seedInputField.text));

        _setupMenu.SetActive(false);
        SceneManager.LoadScene("Game");
    }

    private void OpenSettings()
    {
        _isSettingsOpen = !_isSettingsOpen;
        _settingsMenu.SetActive(_isSettingsOpen);
    }

    private void Quit()
    {
        Application.Quit();
    }

    private void DisplayScorebaord()
    {
        if(scoreboard == null) return;
        _scoreboardMenu.SetActive(true);

        int currentPlace = 1;

        foreach (PlayerProfile profile in scoreboard)
        {
            GameObject scoreObject = new GameObject(profile.nickname);
            TextMeshProUGUI text = scoreObject.AddComponent<TextMeshProUGUI>();

            text.text = currentPlace + ". " + profile.nickname + ": " + profile.score;

            text.fontSize = currentPlace switch
            {
                1 => 32,
                2 or 3 => 26,
                _ => 22
            };

            scoreObject.transform.SetParent(_scoreboardGrid, false);

            currentPlace++;
        }
    }
}
