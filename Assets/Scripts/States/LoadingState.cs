using UnityEngine;

[CreateAssetMenu(fileName = "LoadingState", menuName = "States/LoadingState")]
public class LoadingState : State
{
    private LoadingScreenController _loadingScreenController => LoadingScreenController.Instance;
    private WorldGenerator _worldGenerator => WorldGenerator.Instance;
    private StatesManager _statesManager => StatesManager.Instance;
    private PlayerController _playerController => PlayerController.Instance;

    public override void OnEnterState()
    {
        _worldGenerator.UpdateMap();
        _loadingScreenController.Display();
    }

    public override void OnExitState()
    {
        _loadingScreenController.Hide();
    }

    public override void Tick()
    {
        _worldGenerator.Tick();

        if(_worldGenerator.isWorldLoaded)
        {
            _statesManager.SwitchState<PlayingState>();
            _playerController.OnGameStart();
        }
    }

    public override void Tick05s()
    {
        _loadingScreenController.Tick05s();
    }
}
