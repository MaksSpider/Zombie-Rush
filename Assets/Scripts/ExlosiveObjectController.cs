using UnityEngine;

public class ExlosiveObjectController : ObjectController
{
    [SerializeField] private float explosionRange = 2f;
    [SerializeField] private int explosionDamage = 30;
    [SerializeField] private AnimationCurve explosionCurve = AnimationCurve.EaseInOut(1f, 1f, 0.6f, 0.2f);

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;
        Explosion();
    }

    private void Explosion()
    {

    }
}
