public class StatesManager : StateMachine
{
    public static StatesManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SwitchState<PlayingState>();
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
