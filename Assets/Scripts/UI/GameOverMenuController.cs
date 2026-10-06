using TMPro;
using UnityEngine;

public class GameOverMenuController : MonoBehaviour
{
    public static GameOverMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _gameOverMenu;
    [SerializeField] private TextMeshProUGUI _scoreTitle;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
}
