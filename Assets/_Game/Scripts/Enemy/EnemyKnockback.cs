using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    [SerializeField] private Enemy m_enemy = default;
    [SerializeField] private bool m_enabled = true;
    [SerializeField] private float m_force = 30f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_enabled && collision.CompareTag("Player")) {
            if (PlayerHook.CurrentPlayerState != PlayerHook.PlayerState.Disabled && collision.gameObject.GetComponent<PlayerCombat>().currentAttackState != PlayerCombat.AttackState.Smash) { //collider.gameObject.GetComponent<PlayerCombat>().CurrentlyHit == false
                collision.gameObject.GetComponent<PlayerCombat>().GetHit(transform.position, m_force, m_enemy); //10 --> besseren fix finden
                collision.gameObject.GetComponent<PlayerHook>().CancelHook();
                if (GetComponent<Animator>() != null) {
                    GetComponent<Animator>().SetTrigger("Attack"); //sollte auf jedenfall im anim script sein nur zum test hier
                }
            }
        }
    }
}
