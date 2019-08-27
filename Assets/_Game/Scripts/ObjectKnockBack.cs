using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectKnockBack : MonoBehaviour //only for enemies atm
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private bool m_enabled = false;
    [SerializeField] private float m_force = 25f;
    [SerializeField] private UnityEvent m_onSuccesfulHit = default;

    //*************************//
    //    Private Functions    //
    //*************************//

    private void OnTriggerEnter2D(Collider2D collision) //testen ob das irgendwas behindertes auslöst
    {
        CheckEnemyHit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision) 
    {
        CheckEnemyHit(collision);
    }

    private void CheckEnemyHit(Collider2D collision)
    {
        if (m_enabled && collision.CompareTag("Enemy"))
        {
            m_onSuccesfulHit?.Invoke();
            if (collision.GetComponent<Enemy>().currentEnemyState != Enemy.EnemyState.Dead && collision.GetComponent<Enemy>().invincible == false)
            {
                collision.GetComponent<Enemy>().GetHit(transform.position, m_force, 4);
            }
        }
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void IsLethal(bool _enabled)
    {
        m_enabled = _enabled;
    }
}
