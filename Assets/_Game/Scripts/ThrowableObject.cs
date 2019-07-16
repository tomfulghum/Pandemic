﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableObject : MonoBehaviour
{
    // Start is called before the first frame update
    Vector2 CurrentVelocity;
    public float Gravity;
    public bool PickedUp;
    public bool CurrentlyThrown;
    public Transform ObjectToFollow;
    Vector2 _gravity;
    Actor2D actor;
    void Start()
    {
        actor = GetComponent<Actor2D>();
        _gravity = new Vector2(0, Gravity);
    }

    // Update is called once per frame
    void Update()
    {
        if(CurrentlyThrown)
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;
            if (actor.collision.above || actor.collision.below || actor.collision.left || actor.collision.right)
            {
                actor.velocity = new Vector2(0, 0);
                CurrentVelocity = Vector2.zero;
                CurrentlyThrown = false;
                GetComponent<SpriteRenderer>().color = Color.blue;
            }
            Transform enemy = null;
            if (actor.collision.below && actor.collision.below.CompareTag("Enemy"))
                enemy = actor.collision.below;
            if (actor.collision.above && actor.collision.above.CompareTag("Enemy"))
                enemy = actor.collision.above;
            if (actor.collision.left && actor.collision.left.CompareTag("Enemy"))
                enemy = actor.collision.left;
            if (actor.collision.right && actor.collision.right.CompareTag("Enemy"))
                enemy = actor.collision.right;
            if(enemy != null)
            {
                Debug.Log("hit enemy");
                bool KnockBackLeft = true;
                if (transform.position.x < enemy.position.x)
                    KnockBackLeft = false;
                enemy.GetComponent<Enemy>().GetHit(KnockBackLeft, 0.3f);
            }
        } 
        if (!PickedUp)
        {
            if (actor.collision.above || actor.collision.below)
            {
                actor.velocity = new Vector2(actor.velocity.x, 0);
            }
            if (actor.collision.left || actor.collision.right)
            {
                actor.velocity = new Vector2(0, actor.velocity.y);
            }

            CurrentVelocity += Vector2.up * (-_gravity * Time.deltaTime);
            //transform.position += (Vector3)CurrentVelocity * Time.deltaTime;
            actor.velocity = CurrentVelocity;
        } else
        {
            actor.velocity = Vector2.zero;
           // Debug.Log("picked Up");
            if (ObjectToFollow != null && ObjectToFollow.gameObject.GetComponent<PlayerHook>().PullTargetToPlayer == false) 
            {
                transform.position = ObjectToFollow.position;
            }
        }
    }

    public void Throw(Vector2 _velocity) // nur ein parameter 
    {
        CurrentVelocity = _velocity;
        CurrentlyThrown = true;
    }
}