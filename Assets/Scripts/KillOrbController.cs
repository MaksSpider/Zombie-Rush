using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KillOrbController : MonoBehaviour
{
    private PlayerController playerController;
    private CameraController cameraController;
    private UIController uiController;

    public float speed = 8f;
    public float acceleration = 25f;

    private RectTransform target;
    private Camera cam;

    void Awake()
    {
        playerController = PlayerController.Instance;
        cameraController = CameraController.Instance;
        uiController = UIController.Instance;

        target = uiController.killsIndicator;
        cam = cameraController.cam;
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