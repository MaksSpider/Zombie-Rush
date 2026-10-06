using Unity.VisualScripting;
using UnityEngine;

public class PlayingState : State
{
    private PlayerCombatController _playerCombatController => PlayerCombatController.Instance;
    private PauseMenuController _pauseMenuController => PauseMenuController.Instance;
    private InventoryController _inventoryController => InventoryController.Instance;
    private InteractionsManager _interactionsManager => InteractionsManager.Instance;
    private StatusesController _statusesController => StatusesController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private PlayerController _playerController => PlayerController.Instance;
    private EnemiesManager _enemiesManager => EnemiesManager.Instance;
    private WorldGenerator _worldGenerator => WorldGenerator.Instance;
    private AttacksManager _attacksManager => AttacksManager.Instance;
    private InputManager _inputManager => InputManager.Instance;
    private CursorManager _cursorManager => CursorManager.Instance;

    public override void OnEnterState()
    {
        _interactionsManager.OnEnterState();
        _inventoryController.OnEnterState();
        _playerCombatController.OnEnterState();
        _playerController.OnEnterState();

        _cursorManager.ChangeCurrentDefault("crosshair");
        _cursorManager.SetDefault();

        _inputManager.InputActions.Menu.Escape.performed += _pauseMenuController.Open;
    }

    public override void OnExitState()
    {
        _interactionsManager.OnExitState();
        _inventoryController.OnExitState();
        _playerCombatController.OnExitState();
        _playerController.OnExitState();

        _cursorManager.SetNormal();

        _inputManager.InputActions.Menu.Escape.performed -= _pauseMenuController.Open;
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
        float fixedDeltaTime = Time.fixedDeltaTime;

        _interactionsManager.FixedTick();
        _playerController.FixedTick(fixedDeltaTime);
        _attacksManager.FixedTick(fixedDeltaTime);
        _enemiesManager.FixedTick(fixedDeltaTime);
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
