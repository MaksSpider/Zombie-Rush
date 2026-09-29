using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    private InputManager inputManager;

    [SerializeField] private GameObject player;

    [SerializeField] private Vector3 baseOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Mouse Look-Ahead")]
    [SerializeField] private float mouseInfluence = 0.3f;
    [SerializeField] private float maxMouseOffset = 3f;

    public Camera cam;

    void Start()
    {
        transform.position = player.transform.position + baseOffset;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        inputManager = InputManager.Instance;
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 mouseScreenPos = inputManager.InputActions.Player.MousePosition.ReadValue<Vector2>();
        Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(cam.transform.position.z));
        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos3D);
        mouseWorldPos.z = 0f;

        Vector3 mouseOffset = (mouseWorldPos - player.transform.position) * mouseInfluence;
        mouseOffset = Vector3.ClampMagnitude(mouseOffset, maxMouseOffset);

        Vector3 targetPosition = player.transform.position + baseOffset + mouseOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
