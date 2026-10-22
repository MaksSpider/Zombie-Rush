using System.Collections.Generic;
using UnityEngine;

public class OrbsManager : MonoBehaviour
{
    public static OrbsManager Instance { get; private set; }

    private List<OrbController> _orbs = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public OrbController CreateOrb(GameObject orbPrefab, Vector2 spawnPosition, Vector2 targetPosition, bool bobbingEffect = true, bool instantCollect = false)
    {
        GameObject orbObject = Instantiate(orbPrefab, spawnPosition, Quaternion.identity, gameObject.transform);
        
        OrbController orbCtr = orbObject.GetComponent<OrbController>();

        if(orbCtr == null)
        {
            orbCtr = orbObject.AddComponent<OrbController>();
        }

        orbCtr.Initialize(targetPosition, bobbingEffect, instantCollect);

        _orbs.Add(orbCtr);

        return orbCtr;
    }

    public void Tick()
    {
        for (int i = _orbs.Count - 1; i >= 0; i--)
        {
            OrbController orb = _orbs[i];

            if (orb == null)
            {
                _orbs.RemoveAt(i);
                continue;
            }

            orb.Tick();
        }
    }

    public void Tick1s()
    {
        for (int i = _orbs.Count - 1; i >= 0; i--)
        {
            OrbController orb = _orbs[i];

            if (orb == null)
            {
                _orbs.RemoveAt(i);
                continue;
            }

            orb.Tick1s();
        }
    }
}
