using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenuController : MonoBehaviour
{
    public static GameOverMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _gameOverMenu;
    [SerializeField] private TextMeshProUGUI _scoreTitle;

    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

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
    }

    public void Open(int score)
    {
        _gameOverMenu.SetActive(true);
        _statesManager.SwitchState<PauseState>();

        _scoreTitle.text = "Score: " + score.ToString();
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


    // TODO: implement settings
    private void OpenSettings()
    {

    }

    private void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
