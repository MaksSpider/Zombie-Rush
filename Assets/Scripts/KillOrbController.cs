using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KillOrbController : MonoBehaviour
{
    private CameraController _cameraController;
    private PlayerController _playerController;

    public float speed = 8f;
    public float acceleration = 25f;

    private RectTransform target;
    private Camera cam;

    void Awake()
    {
        _cameraController = CameraController.Instance;
        _playerController = PlayerController.Instance;

        target = _playerController.killsTMP.rectTransform;
        cam = _cameraController.cam;
    }

    void Update()
    {
        FlyToTarget();
    }

    void FlyToTarget()
    {
        if (target == null) return;

        Vector3 targetWorldPos = RectTransformToWorldPos(target, cam);

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