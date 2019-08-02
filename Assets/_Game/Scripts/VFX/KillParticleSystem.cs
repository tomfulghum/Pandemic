using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillParticleSystem : MonoBehaviour
{
    private ParticleSystem thisPS;
    void Start()
    {
        thisPS = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (thisPS && !thisPS.IsAlive()) {
            Destroy(gameObject);
        }
    }
}
