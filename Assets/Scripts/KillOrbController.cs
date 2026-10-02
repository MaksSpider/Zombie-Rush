using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KillOrbController : MonoBehaviour
{
    private CameraController _cameraController;
    private PlayerController _playerController;

    public float speed = 8f;
    public float acceleration = 25f;

    void Awake()
    {
        _cameraController = CameraController.Instance;
        _playerController = PlayerController.Instance;
    }

    void Update()
    {
        FlyToTarget();
    }

    void FlyToTarget()
    {
        if (_playerController.killsTMP.rectTransform == null) return;

        Vector3 targetWorldPos = RectTransformToWorldPos(_playerController.killsTMP.rectTransform, _cameraController.cam);

        speed += acceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWorldPos) < 0.2f)
        {
            Destroy(gameObject);
        }
    }

    Vector3 RectTransformToWorldPos(RectTransform rt, Camera camera)
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        Ray ray = camera.ScreenPointToRay(screenPos);
        return ray.origin + ray.direction * 5f;
    }
}