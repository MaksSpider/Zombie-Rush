using Unity.VisualScripting;
using UnityEngine;

public class PlayingState : State
{
    private PlayerCombatController _playerCombatController;
    private PauseMenuController _pauseMenuController;
    private InventoryController _inventoryController;
    private InteractionsManager _interactionsManager;
    private StatusesController _statusesController;
    private CameraController _cameraController;
    private PlayerController _playerController;
    private EnemiesManager _enemiesManager;
    private WorldGenerator _worldGenerator;
    private InputManager _inputManager;

    [SerializeField] private Texture2D _crosshair;

    private void Awake()
    {
        _playerCombatController = PlayerCombatController.Instance;
        _pauseMenuController = PauseMenuController.Instance;
        _inventoryController = InventoryController.Instance;
        _interactionsManager = InteractionsManager.Instance;
        _statusesController = StatusesController.Instance;
        _playerController = PlayerController.Instance;
        _cameraController = CameraController.Instance;
        _enemiesManager = EnemiesManager.Instance;
        _worldGenerator = WorldGenerator.Instance;
        _inputManager = InputManager.Instance;
    }

    public override void OnEnterState()
    {
        _inputManager.InputActions.Menu.Escape.performed += ctx => _pauseMenuController.Open();

        _interactionsManager.OnEnterState();
        _inventoryController.OnEnterState();
        _playerCombatController.OnEnterState();
        _playerController.OnEnterState();

        Cursor.SetCursor(_crosshair, new Vector2(16, 16), CursorMode.Auto);
    }

    public override void OnExitState()
    {
        _inputManager.InputActions.Menu.Escape.performed -= ctx => _pauseMenuController.Open();

        _interactionsManager.OnExitState();
        _inventoryController.OnExitState();
        _playerCombatController.OnExitState();
        _playerController.OnExitState();

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public override void Tick()
    {
        _cameraController.Tick();
        _interactionsManager.Tick();
        _inventoryController.Tick();
        _playerCombatController.Tick();
        _playerController.Tick();
        _statusesController.Tick();
    }

    public override void FixedTick()
    {
        _interactionsManager.FixedTick();
        _playerController.FixedTick();
    }

    public override void Tick1s()
    {
        _worldGenerator.Tick1s();
        _playerController.Tick1s();
        _enemiesManager.Tick1s();
    }

    public override void Tick5s()
    {
        _playerController.Tick5s();
    }
}
