using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    Actor2D actor;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        actor = GetComponent<Actor2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(actor.velocity.magnitude);
        if(Input.GetAxis("Horizontal") < -0.15f || Input.GetAxis("Horizontal") > 0.15f)
        {
            anim.SetFloat("RunSpeed", actor.velocity.magnitude);
        } else
        {
            anim.SetFloat("RunSpeed", -0.1f);
        }
        if(GetComponent<PlayerCombat>().FacingLeft == false)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        } else
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }
}
