using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
    [SerializeField] private GameObject partSysOnCollision;
    private List<ParticleCollisionEvent> collisionEvents;
    private ParticleSystem thisPS;

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
            Debug.Log("yey");
        }
    }
}