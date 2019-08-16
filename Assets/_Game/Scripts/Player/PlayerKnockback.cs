using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [SerializeField] private float m_force = 30f;

    private bool m_enabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_enabled && collision.CompareTag("Enemy"))
        {
            if (collision.GetComponent<Enemy>().currentEnemyState != Enemy.EnemyState.Dead)
            {
                collision.GetComponent<Enemy>().GetHit(transform.position, m_force, 4);
            }
        }
    }

    public void IsLethal(bool _enabled)
    {
        m_enabled = _enabled;
    }
}
