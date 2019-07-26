using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    bool FacingLeft;

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
        SetFacingDirection();
        if (Input.GetAxis("Horizontal") < -0.15f || Input.GetAxis("Horizontal") > 0.15f)
        {
            anim.SetFloat("RunSpeed", actor.velocity.magnitude);
        }
        else
        {
            anim.SetFloat("RunSpeed", -0.1f);
        }
        UpdateCollider(GetComponent<SpriteRenderer>().flipX);
        if (FacingLeft == false)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;
    }

    void SetFacingDirection() //only for anim //can differ from facing direc in player combat //should change later
    {
        float CurrentJoystickDirection = Input.GetAxis("Horizontal");
        if (CurrentJoystickDirection < 0)
            FacingLeft = true;
        else if (CurrentJoystickDirection > 0)
            FacingLeft = false;
    }

    void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX  && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
