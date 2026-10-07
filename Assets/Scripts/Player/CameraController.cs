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

    [Header("Camera Shake")]
    [SerializeField] private float _shakeDuration = 0.15f;
    [SerializeField] private float _shakeFrequency = 30f;

    [NonSerialized] public Camera cam;

    private float _shakeTimer;
    private float _shakeForce;
    private float _shakeSeed;

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

    public void Shake(float force = 1f)
    {
        _shakeForce = force;
        _shakeTimer = _shakeDuration;

        _shakeSeed = UnityEngine.Random.Range(0f, 1000f);
    }

    void HandleMovement()
    {
        Vector2 mouseScreenPos = _inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();

        Vector3 mouseScreenPos3D = new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            Mathf.Abs(cam.transform.position.z)
        );

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f;

        Vector3 mouseOffset = (mouseWorldPos - _player.transform.position) * _mouseInfluence;

        mouseOffset = Vector3.ClampMagnitude(mouseOffset, _maxMouseOffset);

        Vector3 targetPosition = _player.transform.position + _baseOffset + mouseOffset;

        Vector3 shakeOffset = GetShakeOffset();

        targetPosition += shakeOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            _smoothSpeed * Time.deltaTime
        );
    }

    Vector3 GetShakeOffset()
    {
        if (_shakeTimer <= 0f) return Vector3.zero;

        _shakeTimer -= Time.deltaTime;

        float normalizedTime = _shakeTimer / _shakeDuration;

        float strength = _shakeForce * normalizedTime;

        float time = Time.time * _shakeFrequency;

        float x = Mathf.PerlinNoise(_shakeSeed, time) * 2f - 1f;
        float y = Mathf.PerlinNoise(_shakeSeed + 100f, time) * 2f - 1f;

        return new Vector3(x, y, 0f) * strength;
    }
}