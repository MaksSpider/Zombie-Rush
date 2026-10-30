using TMPro;
using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance;

    private WorldGenerator _worldGenerator => WorldGenerator.Instance;

    [SerializeField] private GameObject _loadingScreenUI;
    [SerializeField] private ProgressBar _progressBar;
    [SerializeField] private TextMeshProUGUI _loadingText;

    private int loadingTextStage = 0;
    [SerializeField] private int tilesToGenerate = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Display()
    {
        tilesToGenerate = _worldGenerator.tilesToLoadQueue.Count;
        _loadingScreenUI.SetActive(true);
        Tick05s();
    }

    public void Hide()
    {
        _loadingScreenUI.SetActive(false);
    }

    public void Tick05s()
    {
        float loadingState = (float)(tilesToGenerate - _worldGenerator.tilesToLoadQueue.Count) / (float)tilesToGenerate;

        _progressBar.value = loadingState;

        switch (loadingTextStage)
        {
            case 0:
                _loadingText.text = "Loading";
                break;
            case 1:
                _loadingText.text = "Loading.";
                break;
            case 2:
                _loadingText.text = "Loading..";
                break;
            case 3:
                _loadingText.text = "Loading...";
                break;
            default:
                loadingTextStage = -1;
                break;
        }

        loadingTextStage++;
    }
}
