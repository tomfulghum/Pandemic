using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayLandingEffect : MonoBehaviour
{
    [SerializeField] public ParticleSystem landingPS;
    private Actor2D player;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Actor2D>();
    }
    //TODO
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.relativeVelocity.magnitude);
        if (!landingPS.isEmitting && collision.relativeVelocity.magnitude > 4)
            landingPS.Play();
    }

}
