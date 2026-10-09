using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenuController : MonoBehaviour
{
    public static GameOverMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;
    private GameSession _gameSession => GameSession.Instance;

    [SerializeField] private GameObject _gameOverMenu;
    [SerializeField] private GameObject _settingsMenu;

    [SerializeField] private TextMeshProUGUI _nicknameTitle;
    [SerializeField] private TextMeshProUGUI _scoreTitle;

    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    private bool isSettingsOpen = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        _restartButton.GetComponent<Button>().onClick.AddListener(RestartGame);
        _settingsButton.GetComponent<Button>().onClick.AddListener(OpenSettings);
        _quitButton.GetComponent<Button>().onClick.AddListener(Quit);

        _settingsMenu.SetActive(false);
        _gameOverMenu.SetActive(false);
    }

    public void Open()
    {
        _gameOverMenu.SetActive(true);
        _statesManager.SwitchState<PauseState>();

        _nicknameTitle.text = _gameSession.playerProfile.nickname;
        _scoreTitle.text = "Score: " + _gameSession.playerProfile.score.ToString();

        _gameOverMenu.transform.Find("Menu").GetComponent<UIEnterAnimations>().EnterAnimation();
    }

    public void Close()
    {
        _gameOverMenu.SetActive(false);
        _statesManager.SwitchState<PlayingState>();
    }


    private void RestartGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void OpenSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        _settingsMenu.SetActive(isSettingsOpen);
    }

    private void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
