using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaguePeasantAnim : MonoBehaviour
{
    //**********************//
    //    Private Fields    //
    //**********************//

    private bool m_triggeredDeath = false;

    private Vector3 m_objectScale;

    private PlaguePeasant m_pp;
    private Enemy m_enemy;
    private Animator m_anim;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_anim = GetComponent<Animator>();
        m_enemy = GetComponent<Enemy>();
        m_pp = GetComponent<PlaguePeasant>();
        m_objectScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        FlipObject();
        if (m_enemy.currentEnemyState == Enemy.EnemyState.Dead && m_triggeredDeath == false)
        {
            m_anim.SetTrigger("Death");
            m_triggeredDeath = true;
        }
        /*
        UpdateCollider(GetComponent<SpriteRenderer>().flipX);
        if (enemy.CurrentMovementDirection == PlaguePeasant.MovementDirection.Left)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;
        */
        if (m_pp.currentMovementState == PlaguePeasant.MovementState.Move || m_pp.currentMovementState == PlaguePeasant.MovementState.Chase) //ist das mit dem float so eine gute idee?
            m_anim.SetBool("Moving", true);
        else
            m_anim.SetBool("Moving", false);

        if (m_pp.currentMovementState == PlaguePeasant.MovementState.Sit) //ist das mit dem float so eine gute idee?
            m_anim.SetBool("Sitting", true);
        else
            m_anim.SetBool("Sitting", false);

        if (m_enemy.currentEnemyState == Enemy.EnemyState.Hit)
            m_anim.SetBool("Hit", true); //später evtl trigger
        else
            m_anim.SetBool("Hit", false);
    }


    //*************************//
    //    Private Functions    //
    //*************************//

    private void FlipObject() //vllt besser in plague peasant?
    {
        if(m_pp.currentMovementDirection == PlaguePeasant.MovementDirection.Left)
            transform.localScale = new Vector3(-m_objectScale.x, m_objectScale.y, m_objectScale.z);
        else
            transform.localScale = new Vector3(m_objectScale.x, m_objectScale.y, m_objectScale.z);
    }

    private void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
