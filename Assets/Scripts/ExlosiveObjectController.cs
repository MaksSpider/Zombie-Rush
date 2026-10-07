using UnityEngine;

public class ExlosiveObjectController : DamageHandler
{
    [SerializeField] private float explosionRange = 2f;
    [SerializeField] private int explosionDamage = 30;
    [SerializeField] private AnimationCurve explosionCurve = AnimationCurve.EaseInOut(1f, 1f, 0.6f, 0.2f);

    // TODO: implement
    public override void OnObjectDestroy()
    {
        explosionRange += 0;
        explosionDamage += 5;
    }
}
