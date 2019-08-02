using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmittParticlesSimultaneously : MonoBehaviour
{

    [SerializeField] private GameObject partSys1;
    [SerializeField] private GameObject partSys2;
    private ParticleSystem sys1;
    private ParticleSystem sys2;
    void Start()
    {
        sys1 = Instantiate(partSys1, transform).GetComponent<ParticleSystem>();
        sys2 = Instantiate(partSys2, transform).GetComponent<ParticleSystem>();
    }
    void Update()
    {
        float probability = Random.Range(0.0f, 1.0f);
        Debug.Log(probability);
        if (probability > 0.8f && !sys1.IsAlive() && !sys2.IsAlive()) {
            sys1.Play();
            sys2.Play();
        }
    }
}