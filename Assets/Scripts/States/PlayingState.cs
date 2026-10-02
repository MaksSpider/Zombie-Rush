using Unity.VisualScripting;
using UnityEngine;

public class PlayingState : State
{
    private PlayerCombatController _playerCombatController => PlayerCombatController.Instance;
    private PauseMenuController _pauseMenuController => PauseMenuController.Instance;
    private InventoryController _inventoryController => InventoryController.Instance;
    private InteractionsManager _interactionsManager => InteractionsManager.Instance;
    private StatusesController _statusesController => StatusesController.Instance;
    private TooltipController _tooltipController => TooltipController.Instance;
    private CameraController _cameraController => CameraController.Instance;
    private PlayerController _playerController => PlayerController.Instance;
    private EnemiesManager _enemiesManager => EnemiesManager.Instance;
    private WorldGenerator _worldGenerator => WorldGenerator.Instance;

    [SerializeField] private Texture2D _crosshair;

    public override void OnEnterState()
    {
        _interactionsManager.OnEnterState();
        _inventoryController.OnEnterState();
        _playerCombatController.OnEnterState();
        _playerController.OnEnterState();

        Cursor.SetCursor(_crosshair, new Vector2(16, 16), CursorMode.Auto);
    }

    public override void OnExitState()
    {
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
