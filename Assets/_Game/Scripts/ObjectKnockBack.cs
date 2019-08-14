using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectKnockBack : MonoBehaviour //only for enemies atm
{
    [SerializeField] private bool m_enabled = false;
    [SerializeField] private float m_force = 25f;
    [SerializeField] private UnityEvent m_onSuccesfulHit = default;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_enabled && collision.CompareTag("Enemy"))
        {
            if (collision.GetComponent<Enemy>().currentEnemyState != Enemy.EnemyState.Dead)
            {
                collision.GetComponent<Enemy>().GetHit(transform.position, m_force, 4);
                m_onSuccesfulHit?.Invoke();
            }
        }
    }

    public void IsLethal(bool _enabled)
    {
        m_enabled = _enabled;
    }
}
