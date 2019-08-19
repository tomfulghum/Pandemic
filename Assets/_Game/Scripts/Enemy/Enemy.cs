﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enemy in eine elternklasse umwandeln die für alle kind enemys deren movementpattern funktion im update aufruft
public class Enemy : MonoBehaviour //vllt anstatt enemy ein allgemeines script schreiben was auch für den player anwendbar ist
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public enum EnemyState //usw. //evtl moving besser namen
    {
        Moving,
        Disabled,
        Hit,
        Dead
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private int m_maxHealth = 6;

    //******************//
    //    Properties    //
    //******************//

    public EnemyState currentEnemyState
    {
        get { return m_currentEnemyState; }
    }

    public bool frozen
    {
        get { return m_frozen; }
        set { m_frozen = value; }
    }

    public bool invincible
    {
        get { return m_invincible; }
        set { m_invincible = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private EnemyState m_currentEnemyState = EnemyState.Moving;

    private int m_currentHealth = 0;

    private bool m_frozen = false;
    private bool m_invincible = false;

    private Coroutine m_enemyKnockBack = null;
    private Actor2D m_actor = null; // vllt reich der actor auf crawling enemy
    private Collider2D m_coll = null;
    private Rigidbody2D m_rb = null;
    private EnemyKnockback m_ekb;
    private SpriteRenderer m_spriteRenderer = null;
    private int m_currentHitPriority = 0;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_coll = GetComponent<Collider2D>();
        m_rb = GetComponent<Rigidbody2D>();
        m_ekb = GetComponentInChildren<EnemyKnockback>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_currentHealth = m_maxHealth;
    }

    private void FixedUpdate()
    {

    }

    void Update()
    {
        if (currentEnemyState != EnemyState.Dead) {
            if (m_currentHealth <= 0) {
                m_actor.velocity = Vector2.zero;
                m_ekb.IsEnemyLethal(false);
                Destroy(gameObject, 2f); //despawn time //evtl länger?
                m_currentEnemyState = EnemyState.Dead;
            }
            /*
            if (currentEnemyState == EnemyState.Hit) {
                m_colorChangeCounter++;
                if (m_colorChangeCounter % 5 == 0)
                    m_spriteRenderer.color = Color.white;
                else
                    m_spriteRenderer.color = m_originalColor;
            }
            */
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    /*
    IEnumerator Despawn() //falls später evtl noch mehr passieren soll
    {

    }
    */

    //besser machen und die schwerkraft usw alles mitberechnen --> evtl in ein anderes script //check collissions evtl auch woanders rein
    //was soll passieren wenn man den gegner / spieler in die wand knockt?
    private IEnumerator KnockBack(Vector2 _knockBackOrigin, float _knockBackForce) //deactivate layer collission? //geht mit dem neuen system von freddie evtl nichtmerh //knockback direction hier festlegen //freeze player -_> eher nciht
    {
        //m_currentHealth--;
        //m_currentEnemyState = EnemyState.Hit;
        //
        //Vector2 direction = ((Vector2)transform.position - _knockBackOrigin).normalized; //veraltet
        //if (_knockBackOrigin.x < transform.position.x)
        //    direction = new Vector2(0.5f, 0.5f).normalized;
        //else
        //    direction = new Vector2(-0.5f, 0.5f).normalized;
        //
        //SetFreeze(true); //overkill?
        //
        //yield return new WaitForSeconds(0.5f); // Freeze time //m_hitFreezeTime
        //
        //m_rb.velocity = direction * _knockBackForce;
        //
        //yield return new WaitForSeconds(0.2f); // Knockback time //m_hitKnockbackTime
        //
        //m_currentHitPriority = 0;
        //m_currentEnemyState = EnemyState.Moving;
        //SetFreeze(false); //overkill?


        m_currentHealth--;
        m_currentEnemyState = EnemyState.Hit;
        for (int i = 0; i < 10; i++) {
            float test = 1 - Mathf.Pow((i), 3) / 100; //warum?
            if (test < 0) {
                test = 0;
            }
            int additionalPosition = 0;
            if (Mathf.Abs(transform.position.x - _knockBackOrigin.x) < 0.15f) { //KnockBacktolerance or so
                additionalPosition = 10;
            }
            Vector2 KnockBackDirection = ((Vector2)transform.position - new Vector2(_knockBackOrigin.x + additionalPosition, _knockBackOrigin.y)).normalized;
            m_rb.velocity = KnockBackDirection * test * _knockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee
            if (m_actor.contacts.above || m_actor.contacts.below) {
                m_rb.velocity = new Vector2(m_rb.velocity.x, 0);
            }
            if (m_actor.contacts.left || m_actor.contacts.right) {
                m_rb.velocity = new Vector2(0, m_rb.velocity.y);
            }
        
            yield return new WaitForSeconds(0.03f);
        }
        m_currentHitPriority = 0;
        m_currentEnemyState = EnemyState.Moving;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void GetHit(Vector2 _knockBackOrigin, float _knockBackForce, int _hitPriority) //bandaid fix for knockbackdirectino //jedesmal move prio checken und dann entscheiden ob man genockbacked wird oder nicht
    {
        if (currentEnemyState == EnemyState.Dead) { //nochmal überprüfen ob das klappt
            return;
        }
        //vllt die überprüfung ob der hit gilt hier rein machen
        if (currentEnemyState != EnemyState.Hit) {
            m_enemyKnockBack = StartCoroutine(KnockBack(_knockBackOrigin, _knockBackForce));
        } else if (currentEnemyState == EnemyState.Hit && _hitPriority > m_currentHitPriority) { //evtl reicht auch >= //ist das wirklich so ein guter ansatz?
            StopCoroutine(m_enemyKnockBack);
            m_enemyKnockBack = StartCoroutine(KnockBack( _knockBackOrigin, _knockBackForce));
        }
        m_currentHitPriority = _hitPriority;
    }

    public void SetFreeze(bool _freeze)
    {
        if(_freeze)
        {
            m_rb.isKinematic = true;
            m_rb.velocity = Vector2.zero;
            m_currentEnemyState = EnemyState.Disabled;
        } else
        {
            m_rb.isKinematic = false;
            m_currentEnemyState = EnemyState.Moving;
        }
    }
}
