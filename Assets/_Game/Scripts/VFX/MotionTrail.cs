using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionTrail : MonoBehaviour
{
    [SerializeField] private Actor2D actor;

    void Start()
    {
        actor = GetComponent<Actor2D>();
    }
    
    void Update()
    {
        Vector2 vel = actor.velocity;
    }
}
