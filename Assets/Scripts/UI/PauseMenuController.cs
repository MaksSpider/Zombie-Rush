using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    private StatesManager _statesManager => StatesManager.Instance;

    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _settingsMenu;

    [SerializeField] private Button _mainMenuButton;
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
        _mainMenuButton.GetComponent<Button>().onClick.AddListener(MainMenu);
        _settingsButton.GetComponent<Button>().onClick.AddListener(OpenSettings);
        _quitButton.GetComponent<Button>().onClick.AddListener(Quit);

        _settingsMenu.SetActive(false);
        _pauseMenu.SetActive(false);
    }

    public void Open(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _pauseMenu.SetActive(true);
        _statesManager.SwitchState<PauseState>();
        _pauseMenu.transform.Find("Menu").GetComponent<UIEnterAnimations>().EnterAnimation();
    }

    public void Close(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        _pauseMenu.transform.Find("Menu").GetComponent<UIEnterAnimations>().ExitAnimation();
        StartCoroutine(AfterCloseAnimation());
    }

    private IEnumerator AfterCloseAnimation()
    {
        yield return new WaitForSeconds(0.3f);

        _pauseMenu.SetActive(false);
        _statesManager.SwitchState<PlayingState>();
    }

    private void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }


    private void OpenSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        _settingsMenu.SetActive(isSettingsOpen);
    }

    private void Quit()
    {
        Application.Quit();
    }
}
