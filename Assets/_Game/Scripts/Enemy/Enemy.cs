using System.Collections;
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
        Hit,
        Dead
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private bool m_contactDamage = false;
    [SerializeField] private int m_maxHealth = 6;

    //******************//
    //    Properties    //
    //******************//

    public EnemyState currentEnemyState
    {
        get { return m_currentEnemyState; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private EnemyState m_currentEnemyState = EnemyState.Moving;

    private int m_currentHealth = 0;
    private int m_colorChangeCounter = 0;

    private Color m_originalColor = default;

    private Coroutine m_enemyKnockBack = null;
    private Actor2D m_actor = null; // vllt reich der actor auf crawling enemy
    private Rigidbody2D m_rb = null;
    private SpriteRenderer m_spriteRenderer = null;
    private int m_currentHitPriority = 0;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_actor = GetComponent<Actor2D>();
        m_rb = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_originalColor = GetComponent<SpriteRenderer>().color;
        m_currentHealth = m_maxHealth;
    }

    void Update()
    {
        if (currentEnemyState != EnemyState.Dead) {
            if (m_currentHealth <= 0) {
                m_actor.velocity = Vector2.zero;
                Destroy(gameObject, 2f); //despawn time //evtl länger?
                m_currentEnemyState = EnemyState.Dead;
            }
            if (currentEnemyState == EnemyState.Hit) {
                m_colorChangeCounter++;
                if (m_colorChangeCounter % 5 == 0)
                    m_spriteRenderer.color = Color.white;
                else
                    m_spriteRenderer.color = m_originalColor;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (m_contactDamage) {
            var other = collision.collider;

            if (other.CompareTag("Player")) {
                if (PlayerHook.CurrentPlayerState != PlayerHook.PlayerState.Disabled && other.gameObject.GetComponent<PlayerCombat>().currentAttackState != PlayerCombat.AttackState.Smash) { //collider.gameObject.GetComponent<PlayerCombat>().CurrentlyHit == false
                    other.gameObject.GetComponent<PlayerCombat>().GetHit(transform, 30); //10 --> besseren fix finden
                    other.gameObject.GetComponent<PlayerHook>().CancelHook();
                    if (GetComponent<Animator>() != null) {
                        GetComponent<Animator>().SetTrigger("Attack"); //sollte auf jedenfall im anim script sein nur zum test hier
                    }
                }
            }
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
    private IEnumerator KnockBack(float _repetitions, Transform _knockBackOrigin, float _knockBackForce) //deactivate layer collission? //geht mit dem neuen system von freddie evtl nichtmerh //knockback direction hier festlegen
    {
        m_currentHealth--;
        m_currentEnemyState = EnemyState.Hit;
        for (int i = 0; i < _repetitions; i++) {
            float test = 1 - Mathf.Pow((i), 3) / 100; //warum?
            if (test < 0) {
                test = 0;
            }
            int additionalPosition = 0;
            if (Mathf.Abs(transform.position.x - _knockBackOrigin.position.x) < 0.15f) { //KnockBacktolerance or so
                additionalPosition = 10;
            }
            Vector2 KnockBackDirection = (transform.position - new Vector3(_knockBackOrigin.position.x + additionalPosition, _knockBackOrigin.position.y, _knockBackOrigin.position.z)).normalized;
            m_rb.velocity = KnockBackDirection * test * _knockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee
            if (m_actor.contacts.above || m_actor.contacts.below) {
                m_rb.velocity = new Vector2(m_rb.velocity.x, 0);
            }
            if (m_actor.contacts.left || m_actor.contacts.right) {
                m_rb.velocity = new Vector2(0, m_rb.velocity.y);
            }

            yield return new WaitForSeconds(0.03f);
        }
        m_spriteRenderer.color = m_originalColor;
        m_colorChangeCounter = 0;
        m_currentHitPriority = 0;
        m_currentEnemyState = EnemyState.Moving;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void GetHit(Transform _knockBackOrigin, float _knockBackForce, int _hitPriority) //bandaid fix for knockbackdirectino //jedesmal move prio checken und dann entscheiden ob man genockbacked wird oder nicht
    {
        if (currentEnemyState == EnemyState.Dead) { //nochmal überprüfen ob das klappt
            return;
        }
        //vllt die überprüfung ob der hit gilt hier rein machen
        if (currentEnemyState != EnemyState.Hit) {
            m_enemyKnockBack = StartCoroutine(KnockBack(10, _knockBackOrigin, _knockBackForce));
        } else if (currentEnemyState == EnemyState.Hit && _hitPriority > m_currentHitPriority) { //evtl reicht auch >= //ist das wirklich so ein guter ansatz?
            StopCoroutine(m_enemyKnockBack);
            m_enemyKnockBack = StartCoroutine(KnockBack(10, _knockBackOrigin, _knockBackForce));
        }
        m_currentHitPriority = _hitPriority;
    }
}
