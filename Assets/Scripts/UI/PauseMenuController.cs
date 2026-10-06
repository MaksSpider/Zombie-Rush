using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _pauseMenu;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Open(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _pauseMenu.SetActive(true);
        _statesManager.SwitchState<PauseState>();
    }

    public void Close(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _pauseMenu.SetActive(false);
        _statesManager.SwitchState<PlayingState>();
    }
}
