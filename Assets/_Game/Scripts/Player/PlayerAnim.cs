using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{

    //**********************//
    //    Private Fields    //
    //**********************//

    private bool m_facingLeft;

    private Actor2D m_actor;
    private Rigidbody2D m_rb;
    private Animator m_anim;
    private PlayerCombat m_pc;


    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<Animator>();
        m_pc = GetComponent<PlayerCombat>();
    }

    // Update is called once per frame
    private void Update()
    {
        SetFacingDirection();
        if (Input.GetAxis("Horizontal") < -0.15f || Input.GetAxis("Horizontal") > 0.15f)
        {
            m_anim.SetFloat("RunSpeed", m_rb.velocity.magnitude);
        }
        else
        {
            m_anim.SetFloat("RunSpeed", -0.1f);
        }
        if(m_pc.currentAttackState == PlayerCombat.AttackState.Dash)
        {
            m_anim.SetBool("Dash", true);
        } else
        {
            m_anim.SetBool("Dash", false);
        }

        UpdateCollider(GetComponent<SpriteRenderer>().flipX);
        if (m_pc.currentAttackState != PlayerCombat.AttackState.Dash)
        {
            if (m_facingLeft == false)
                GetComponent<SpriteRenderer>().flipX = false;
            else
                GetComponent<SpriteRenderer>().flipX = true;
        }
    }



    //*************************//
    //    Private Functions    //
    //*************************//


    private void SetFacingDirection() //only for anim //can differ from facing direc in player combat //should change later
    {
        float CurrentJoystickDirection = Input.GetAxis("Horizontal");
        if (CurrentJoystickDirection < 0)
            m_facingLeft = true;
        else if (CurrentJoystickDirection > 0)
            m_facingLeft = false;
    }

    private void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX  && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
