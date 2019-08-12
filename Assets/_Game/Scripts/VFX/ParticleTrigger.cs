using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
    [SerializeField] private GameObject partSysOnCollision = default;
    private List<ParticleCollisionEvent> collisionEvents = default;
    private ParticleSystem thisPS = default;

    private void Start()
    {
        thisPS = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }
    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = thisPS.GetCollisionEvents(other, collisionEvents);
        for (int i = 0; i < numCollisionEvents; i++) {
            Vector2 newPos = collisionEvents[i].intersection;
            GameObject obj = Instantiate(partSysOnCollision, newPos, transform.rotation);
            obj.GetComponent<ParticleSystem>().Play();
            //Debug.Log("yey");
        }
    }
}