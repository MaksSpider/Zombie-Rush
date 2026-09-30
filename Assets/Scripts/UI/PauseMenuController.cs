using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    private StatesManager _statesManager;

    private bool _open = false;

    [SerializeField] private GameObject _pauseMenu;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _statesManager = StatesManager.Instance;
    }

    public void Switch()
    {
        if(_open)
        {
            Close();
            return;
        }

        Open();
    }

    public void Open()
    {
        _open = true;
        _pauseMenu.SetActive(true);
        _statesManager.SwitchState<PauseState>();
    }

    public void Close()
    {
        _open = false;
        _pauseMenu.SetActive(false);
        _statesManager.SwitchState<PlayingState>();
    }
}
