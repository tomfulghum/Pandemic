using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//current player state hier rein mit puclic function set / get player state
public class PlayerAnim : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum PlayerState
    {
        Waiting,
        Hook,
        Attacking,
        Moving,
        Disabled,
        Invincible,
        Dead
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private GameObject m_landingAnimPrefab;

    //******************//
    //    Properties    //
    //******************//

    public PlayerState currentPlayerState
    {
        get { return m_currentPlayerState; }
        set { m_currentPlayerState = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private PlayerState m_currentPlayerState = PlayerState.Waiting;

    private bool m_facingLeft;

    private Actor2D m_actor;
    private Rigidbody2D m_rb;
    private Animator m_anim;
    private PlayerCombat m_pc;
    private PlayerMovement m_pm;

    private bool m_startedMoving;
    private bool m_landing;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<Animator>();
        m_pc = GetComponent<PlayerCombat>();
        m_pm = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    private void Update()
    {
        m_anim.SetFloat("VerticalVelocity", m_rb.velocity.y);
        if(m_anim.GetBool("Grounded") != m_actor.contacts.below)
        {
            m_anim.SetBool("Grounded", m_actor.contacts.below);
            if(m_anim.GetBool("Grounded") == true)
            {
                InstantiateLandingEffect();
            }
        }
        //m_anim.SetBool("Grounded", m_actor.contacts.below);

        if (m_actor.contacts.below || m_currentPlayerState == PlayerState.Disabled || m_pc.currentAttackState == PlayerCombat.AttackState.Dash)
        {
            m_anim.SetBool("JumpActive", false);
        }

        if (m_pm.inputState.movement != Vector2.zero && m_actor.contacts.below && m_pc.currentAttackState != PlayerCombat.AttackState.Dash && m_currentPlayerState != PlayerState.Disabled)
            m_anim.SetBool("Moving", true);
        else
            m_anim.SetBool("Moving", false);

        if (m_currentPlayerState == PlayerState.Disabled)
            m_anim.SetBool("Hit", true);
        else
            m_anim.SetBool("Hit", false);

        if (m_pc.currentAttackState == PlayerCombat.AttackState.Dash)
        {
            m_anim.SetFloat("DashAngle", 0.5f);
            m_anim.SetBool("Dash", true);
        }
        else
            m_anim.SetBool("Dash", false);

        UpdateCollider(m_facingLeft); //evtl auch ! facing left
        if (m_pc.currentAttackState != PlayerCombat.AttackState.Dash && m_currentPlayerState != PlayerState.Disabled)
        {
            SetFacingDirection();
            if (m_facingLeft)
                m_anim.SetFloat("FacingLeft", 1f);
            else
                m_anim.SetFloat("FacingLeft", 0f);
        }
    }



    //*************************//
    //    Private Functions    //
    //*************************//

    private void InstantiateLandingEffect()
    {
        Vector2 effectPosition = GetComponent<BoxCollider2D>().bounds.center;
        effectPosition.y -= GetComponent<BoxCollider2D>().bounds.extents.y;
        GameObject landingEffect = Instantiate(m_landingAnimPrefab, effectPosition, gameObject.transform.rotation);
        Destroy(landingEffect, landingEffect.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
    }


    private void SetFacingDirection() //only for anim //can differ from facing direc in player combat //should change later 
    {
        float CurrentJoystickDirection = m_pm.inputState.movement.x;
        if (CurrentJoystickDirection < 0)
            m_facingLeft = true;
        else if (CurrentJoystickDirection > 0)
            m_facingLeft = false;
    }

    private void UpdateCollider(bool _flipX) //update collider for running --> umändern
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void TriggerJumpAnim()
    {
        m_anim.SetBool("JumpActive", true);
        m_anim.SetTrigger("Jump");
    }

    public void SetDashDirection(Vector2 _dashVelocity)
    {
        float angle;
        if (m_facingLeft)
            angle = Vector2.SignedAngle(_dashVelocity, Vector2.left);
        else
            angle = -Vector2.SignedAngle(_dashVelocity, Vector2.right);

        //Debug.Log("angle: " + angle);
        //Debug.Log("facing left: " + m_facingLeft);

        if (angle <= 22.5f && angle >= -22.5f) //dash left / right
        {
            m_anim.SetFloat("DashAngle", 0.5f);
        }
        else if (angle > 22.5f && angle <= 67.5f) //up 45
        {
            m_anim.SetFloat("DashAngle", 0.25f);
        }
        else if (angle > 67.5f && angle <= 111.5f) //up 90
        {
            m_anim.SetFloat("DashAngle", 0f);
        }
        else if (angle < -22.5f && angle >= -67.5f) //down 45
        {
            m_anim.SetFloat("DashAngle", 0.75f);
        }
        else if (angle < -67.5f && angle >= -111.5f) //down 90
        {
            m_anim.SetFloat("DashAngle", 1f);
        }
    }
}
