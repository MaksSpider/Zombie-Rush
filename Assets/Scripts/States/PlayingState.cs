using UnityEngine;

[CreateAssetMenu(fileName = "PlayingState", menuName = "States/PlayingState")]
public class PlayingState : State
{
    private PlayerCombatController _playerCombatController => PlayerCombatController.Instance;
    private PauseMenuController _pauseMenuController => PauseMenuController.Instance;
    private InventoryController _inventoryController => InventoryController.Instance;
    private InteractionsManager _interactionsManager => InteractionsManager.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private PlayerController _playerController => PlayerController.Instance;
    private EnemiesManager _enemiesManager => EnemiesManager.Instance;
    private WorldGenerator _worldGenerator => WorldGenerator.Instance;
    private AttacksManager _attacksManager => AttacksManager.Instance;
    private CursorManager _cursorManager => CursorManager.Instance;
    private InputManager _inputManager => InputManager.Instance;
    private OrbsManager _orbsManager => OrbsManager.Instance;

    public override void OnEnterState()
    {
        _playerCombatController.OnEnterState();
        _interactionsManager.OnEnterState();
        _inventoryController.OnEnterState();
        _playerController.OnEnterState();

        _cursorManager.ChangeCurrentDefault("crosshair");
        _cursorManager.SetDefault();

        _inputManager.InputActions.Menu.Escape.performed += _pauseMenuController.Open;
    }

    public override void OnExitState()
    {
        _playerCombatController.OnExitState();
        _interactionsManager.OnExitState();
        _inventoryController.OnExitState();
        _playerController.OnExitState();

        _cursorManager.SetNormal();

        _inputManager.InputActions.Menu.Escape.performed -= _pauseMenuController.Open;
    }

    public override void Tick()
    {
        _playerCombatController.Tick();
        _inventoryController.Tick();
        _interactionsManager.Tick();
        _cameraController.Tick();
        _playerController.Tick();
        _worldGenerator.Tick();
        _orbsManager.Tick();
    }

    public override void FixedTick()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;

        _playerController.FixedTick(fixedDeltaTime);
        _attacksManager.FixedTick(fixedDeltaTime);
        _enemiesManager.FixedTick(fixedDeltaTime);
        _interactionsManager.FixedTick();
    }

    public override void Tick1s()
    {
        _playerController.Tick1s();
        _worldGenerator.Tick1s();
        _enemiesManager.Tick1s();
        _orbsManager.Tick1s();
    }

    public override void Tick5s()
    {
        _playerController.Tick5s();
        _enemiesManager.Tick5s();
    }

    public override void Tick10s()
    {
        _playerController.Tick10s();
    }

    public override void Tick30s()
    {
        _playerController.Tick30s();
    }

    public override void Tick1m()
    {
        _playerController.Tick1m();
    }
}
