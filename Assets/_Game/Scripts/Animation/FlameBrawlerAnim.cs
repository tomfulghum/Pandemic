﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBrawlerAnim : MonoBehaviour
{
    //**********************//
    //    Private Fields    //
    //**********************//

    private bool m_triggeredDeath = false;

    private FlameBrawler m_fb;
    private Enemy m_enemy;
    private Animator m_anim;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_anim = GetComponent<Animator>();
        m_enemy = GetComponent<Enemy>();
        m_fb = GetComponent<FlameBrawler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_enemy.currentEnemyState == Enemy.EnemyState.Dead && m_triggeredDeath == false)
        {
            if (m_fb.currentMovementDirection == FlameBrawler.MovementDirection.Right)
                GetComponent<SpriteRenderer>().flipX = true; //nochmal anschaeun
            m_anim.SetTrigger("Death");
            m_triggeredDeath = true;
        }
        if (m_fb.currentMovementState == FlameBrawler.MovementState.Block)
            m_anim.SetBool("Blocking", true);
        else
            m_anim.SetBool("Blocking", false);

        if (m_fb.currentMovementState == FlameBrawler.MovementState.Move || m_fb.currentMovementState == FlameBrawler.MovementState.Chase)
            m_anim.SetBool("Moving", true);
        else
            m_anim.SetBool("Moving", false);

        if (m_fb.currentMovementDirection == FlameBrawler.MovementDirection.Left)
        {
            UpdateCollider(false);
            m_anim.SetFloat("FacingLeft", 1f);
        }
        else
        {
            UpdateCollider(true);
            m_anim.SetFloat("FacingLeft", 0f);
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void UpdateCollider(bool _facingRight) //sieh andere scripte //nochmal direction checken
    {
        if (_facingRight && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_facingRight && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
