using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startButton.GetComponent<Button>().onClick.AddListener(StartGame);
        _settingsButton.GetComponent<Button>().onClick.AddListener(OpenSettings);
        _quitButton.GetComponent<Button>().onClick.AddListener(Quit);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Game");
    }


    // to do implement settings
    private void OpenSettings()
    {

    }

    private void Quit()
    {
        Application.Quit();
    }
}
