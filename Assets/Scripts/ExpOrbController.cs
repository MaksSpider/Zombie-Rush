using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpOrbController : MonoBehaviour
{
    private PlayerController _playerController;
    private CameraController _cameraController;

    public float speed = 8f;
    public float acceleration = 25f;
    public int expValue = 10;

    [SerializeField] private List<AudioClip> _collectSounds;

    [Header("Bobbing")]
    public float bobAmplitude = 0.15f;
    public float bobFrequency = 4f;

    [Header("Burst")]
    [SerializeField] private float burstDuration = 0.3f;
    [SerializeField] private AnimationCurve burstCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optimization")]
    [SerializeField] private float activationRadius = 10f;
    [SerializeField] private float proximityCheckInterval = 2f;

    private RectTransform target;
    private Camera cam;
    private bool isCollecting = false;
    private bool isBursting = false;
    private float bobTimer = 0f;
    private Vector3 spawnPosition;

    void Awake()
    {
        _playerController = PlayerController.Instance;
        _cameraController = CameraController.Instance;

        target = _playerController.levelXpFillMask;
        cam = _cameraController.cam;
    }

    void Start()
    {
        spawnPosition = transform.position;
        bobTimer = Random.Range(0f, 10f);

        float randomOffset = Random.Range(0f, proximityCheckInterval);
        InvokeRepeating(nameof(CheckProximity), randomOffset, proximityCheckInterval);
        enabled = false;
    }

    void Update()
    {
        bobTimer += Time.deltaTime;

        if (isCollecting)
        {
            FlyToTarget();
        }
        else if (!isBursting)
        {
            IdleBob();
        }
    }

    private void CheckProximity()
    {
        if (isCollecting || isBursting)
        {
            enabled = true;
            return;
        }

        float distSqr = (transform.position - _playerController.transform.position).sqrMagnitude;
        enabled = distSqr <= activationRadius * activationRadius;
    }

    public void Burst(Vector3 targetPosition)
    {
        enabled = true;
        isBursting = true;
        StartCoroutine(BurstRoutine(targetPosition));
    }

    private IEnumerator BurstRoutine(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float t = 0f;

        while (t < burstDuration)
        {
            t += Time.deltaTime;
            float progress = burstCurve.Evaluate(t / burstDuration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        transform.position = targetPosition;
        spawnPosition = targetPosition;
        isBursting = false;
    }

    private void IdleBob()
    {
        float yOffset = Mathf.Sin(bobTimer * bobFrequency) * bobAmplitude;
        transform.position = spawnPosition + new Vector3(0f, yOffset, 0f);
    }

    private void FlyToTarget()
    {
        if (target == null) return;

        Vector3 targetWorldPos = RectTransformToWorldPos(target, cam);

        speed += acceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.2f)
        {
            _playerController.ChangeExperience(expValue);
            Destroy(gameObject);
        }
    }

    private Vector3 RectTransformToWorldPos(RectTransform rt, Camera camera)
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        Ray ray = camera.ScreenPointToRay(screenPos);
        return ray.origin + ray.direction * 5f;
    }

    public void Collect()
    {
        if(isCollecting) return;

        if (_collectSounds.Count > 0)
        {
            AudioClip randomClip = _collectSounds[Random.Range(0, _collectSounds.Count)];
            gameObject.GetComponent<AudioSource>().PlayOneShot(randomClip);
        }

        isCollecting = true;
        enabled = true;
    }
}