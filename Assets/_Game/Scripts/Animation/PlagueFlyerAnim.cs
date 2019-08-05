using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueFlyerAnim : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private GameObject m_skull;

    //**********************//
    //    Private Fields    //
    //**********************//

    private bool m_triggeredDeath = false;

    private Borb m_enemy; 
    private Animator m_anim;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_enemy = GetComponent<Borb>();
        m_anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_enemy.GetComponent<Enemy>().currentEnemyState == Enemy.EnemyState.Dead && m_triggeredDeath == false)
        {
            m_anim.SetTrigger("Death");
            m_triggeredDeath = true;
        }

        if (m_enemy.GetComponent<Enemy>().currentEnemyState == Enemy.EnemyState.Hit)
            m_anim.SetBool("Hit", true); //später evtl trigger
        else
            m_anim.SetBool("Hit", false);

        if (m_enemy.currentMovementState == Borb.MovementState.Chase)
            m_anim.SetFloat("AngryFlight", 1);
        else
            m_anim.SetFloat("AngryFlight", 0);

        if (m_enemy.currentMovementState == Borb.MovementState.Nosedive)
            m_anim.SetBool("Dive", true);
        else
            m_anim.SetBool("Dive", false);

        if (m_enemy.currentMovementState == Borb.MovementState.Dazed)
            m_anim.SetBool("Stuck", true);
        else
            m_anim.SetBool("Stuck", false);

        UpdateCollider(GetComponent<SpriteRenderer>().flipX);
        if (m_enemy.currentMovementDirection == Borb.MovementDirection.Left)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void Death()
    {
        GameObject PlagueFlyerSkull = Instantiate(m_skull, transform.position, transform.rotation);
        if (GetComponent<SpriteRenderer>().flipX == true)
        {
            PlagueFlyerSkull.GetComponent<SpriteRenderer>().flipX = true;
            PlagueFlyerSkull.GetComponent<Collider2D>().offset = new Vector2(PlagueFlyerSkull.GetComponent<Collider2D>().offset.x * -1, PlagueFlyerSkull.GetComponent<Collider2D>().offset.y);
        }
        Destroy(gameObject);
    }

    private void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
