using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbController : MonoBehaviour
{
    private PlayerController _playerController => PlayerController.Instance;
    private CameraController _cameraController => CameraController.Instance;

    [Header("Collecting")]
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _acceleration = 25f;

    [Header("Bobbing")]
    [SerializeField] private float _bobAmplitude = 0.15f;
    [SerializeField] private float _bobFrequency = 4f;

    [Header("Burst")]
    [SerializeField] private float _burstDuration = 0.3f;
    [SerializeField] private AnimationCurve _burstCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optimization")]
    [SerializeField] private float _activationRadius = 10f;
    [SerializeField] private float _proximityCheckInterval = 2f;

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> _collectSounds = new();

    enum State
    {
        bursting,
        idle,
        collecting
    }

    private State state;

    private float _bobTimer = 0f;
    private float _burstTimer = 0f;
    private Vector2 _position;
    private Vector2 _spawnPosition;

    [NonSerialized] public RectTransform target;

    public void Initialize(Vector2 targetPosition, bool burst = false, bool instantCollect = false)
    {
        OnInitialized();

        _spawnPosition = transform.position;
        _position = targetPosition;
        _bobTimer = UnityEngine.Random.Range(0f, 10f);

        float _timer = UnityEngine.Random.Range(0f, _proximityCheckInterval);
        enabled = false;

        if (instantCollect)
        {
            Collect();
            return;
        }

        if (burst)
        {
            enabled = true;
            state = State.bursting;
        }
    }

    protected virtual void OnInitialized()
    {

    }

    public void Tick()
    {
        if (!enabled) return;

        _bobTimer += Time.deltaTime;

        if (state == State.idle)
        {
            IdleBob();
            return;
        }

        if(state == State.bursting)
        {
            Burst();
            return;
        }

        if (state == State.collecting)
        {
            if (target == null)
            {
                FlyToPlayer();
                return;
            }

            FlyToTarget();
            return;
        }
    }

    public void Tick1s()
    {
        CheckProximity();
    }

    public void Collect()
    {
        if (state == State.collecting) return;

        state = State.collecting;
        enabled = true;

        if (_collectSounds.Count > 0)
        {
            AudioClip randomClip = _collectSounds[UnityEngine.Random.Range(0, _collectSounds.Count)];
            gameObject.GetComponent<AudioSource>().PlayOneShot(randomClip);
        }
    }

    public virtual void OnCollect()
    {
        Destroy(gameObject);
    }

    private void IdleBob()
    {
        float yOffset = Mathf.Sin(_bobTimer * _bobFrequency) * _bobAmplitude;
        transform.position = _position + new Vector2(0f, yOffset);
    }

    private void FlyToTarget()
    {
        Vector3 targetWorldPos = RectTransformToWorldPos(target, _cameraController.cam);

        _speed += _acceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.2f)
        {
            OnCollect();
        }
    }

    private void FlyToPlayer()
    {
        _speed += _acceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, _playerController.transform.position, _speed * Time.deltaTime);

        if (Vector2.Distance(gameObject.transform.position, _playerController.transform.position) < 0.01f)
        {
            OnCollect();
        }
    }

    private void Burst()
    {
        _burstTimer += Time.deltaTime;
        float progress = _burstCurve.Evaluate(_burstTimer / _burstDuration);
        transform.position = Vector3.Lerp(_spawnPosition, _position, progress);

        if (_burstTimer > _burstDuration)
        {
            transform.position = _position;
            state = State.idle;
        }
    }

    private void CheckProximity()
    {
        if (state == State.collecting || state == State.bursting)
        {
            enabled = true;
            return;
        }

        enabled = Vector2.Distance(transform.position, _playerController.transform.position) <= _activationRadius * _activationRadius;
    }

    private Vector3 RectTransformToWorldPos(RectTransform rt, Camera camera)
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        Ray ray = camera.ScreenPointToRay(screenPos);
        return ray.origin + ray.direction * 5f;
    }
}
