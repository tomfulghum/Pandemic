using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    bool FacingLeft;
    float xAxis;

    Actor2D actor;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        actor = GetComponent<Actor2D>();
        anim = GetComponent<Animator>();
        xAxis = Input.GetAxis("Horizontal");
    }

    // Update is called once per frame
    void Update()
    {
        SetFacingDirection();
        if(Input.GetAxis("Horizontal") < -0.15f || Input.GetAxis("Horizontal") > 0.15f)
        {
            anim.SetFloat("RunSpeed", actor.velocity.magnitude);
        } else
        {
            anim.SetFloat("RunSpeed", -0.1f);
        }
        if(FacingLeft == false)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;
    }

    void SetFacingDirection() //only for anim //can differ from facing direc in player combat //should change later
    {
        float CurrentJoystickDirection = Input.GetAxis("Horizontal");
        if (CurrentJoystickDirection != xAxis)
        {
            if (CurrentJoystickDirection < 0)
                FacingLeft = true;
            else if (CurrentJoystickDirection > 0)
                FacingLeft = false;
            xAxis = CurrentJoystickDirection;
        }
    }

}
