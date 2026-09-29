using Unity.VisualScripting;
using UnityEngine;

public class PlayingState : State
{
    private InputManager _inputManager;

    //[SerializeField] PlayerController player;
    //[SerializeField] EnemiesManager enemiesManager;

    private void Awake()
    {
        _inputManager = InputManager.Instance;
    }

    public override void UpdateState()
    {
        base.UpdateState();

        // update all stuff
    }
}
