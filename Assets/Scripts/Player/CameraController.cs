using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private InputManager _inputManager;


    [SerializeField] private GameObject _player;

    [SerializeField] private Vector3 _baseOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float _smoothSpeed = 5f;

    [Header("Mouse Look-Ahead")]
    [SerializeField] private float _mouseInfluence = 0.3f;
    [SerializeField] private float _maxMouseOffset = 3f;

    public Camera cam;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _inputManager = InputManager.Instance;

        cam = GetComponent<Camera>();
    }

    void Start()
    {
        transform.position = _player.transform.position + _baseOffset;
    }

    public void Tick()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 mouseScreenPos = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
        Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(cam.transform.position.z));
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f;

        Vector3 mouseOffset = (mouseWorldPos - _player.transform.position) * _mouseInfluence;
        mouseOffset = Vector3.ClampMagnitude(mouseOffset, _maxMouseOffset);

        Vector3 targetPosition = _player.transform.position + _baseOffset + mouseOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, _smoothSpeed * Time.deltaTime);
    }
}
