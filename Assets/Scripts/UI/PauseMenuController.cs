using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _pauseMenu;

    [SerializeField] private Button _mainMenuButton;
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
        _mainMenuButton.GetComponent<Button>().onClick.AddListener(MainMenu);
        _settingsButton.GetComponent<Button>().onClick.AddListener(OpenSettings);
        _quitButton.GetComponent<Button>().onClick.AddListener(Quit);
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

    private void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
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
