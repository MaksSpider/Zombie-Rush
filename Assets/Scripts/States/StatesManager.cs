public class StatesManager : StateMachine
{
    public static StatesManager Instance;

    private AudioManager _audioManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioManager = AudioManager.Instance;
    }

    private void Start()
    {
        _audioManager.PlayMusic("song_1");
        SwitchState<LoadingState>();
    }

    private void Update()
    {
        UpdateStateMachine();
    }

    private void FixedUpdate()
    {
        UpdateFixedStateMachine();
    }
}
