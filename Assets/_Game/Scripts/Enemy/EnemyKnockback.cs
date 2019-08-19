using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyKnockback : MonoBehaviour
{
    [SerializeField] private Enemy m_enemy = default;
    [SerializeField] private bool m_enabled = true;
    [SerializeField] private float m_force = 30f;
    [SerializeField] private Animator m_anim = default;
    [SerializeField] private UnityEvent m_onSuccesfulHit = default;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_enabled && collision.CompareTag("Player")) {
            PlayerAnim pa = collision.GetComponent<PlayerAnim>();
            if (pa.currentPlayerState != PlayerAnim.PlayerState.Disabled && pa.currentPlayerState != PlayerAnim.PlayerState.Invincible) { //collider.gameObject.GetComponent<PlayerCombat>().CurrentlyHit == false
                collision.gameObject.GetComponent<PlayerCombat>().GetHit(transform.parent.position, m_force, m_enemy); //10 --> besseren fix finden
                m_onSuccesfulHit?.Invoke();
                if (m_anim != null) {
                    m_anim.SetTrigger("Attack"); //sollte auf jedenfall im anim script sein nur zum test hier
                }
            }
        }
    }

    public void IsEnemyLethal(bool _enabled)
    {
        m_enabled = _enabled;
    }
}
