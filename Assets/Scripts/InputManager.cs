using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public InputSystemActions InputActions { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InputActions = new InputSystemActions();
    }

    private void OnEnable() => InputActions.Enable();
    private void OnDisable() => InputActions.Disable();
}