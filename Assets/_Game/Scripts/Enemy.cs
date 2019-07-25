using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enemy in eine elternklasse umwandeln die für alle kind enemys deren movementpattern funktion im update aufruft
public class Enemy : MonoBehaviour //vllt anstatt enemy ein allgemeines script schreiben was auch für den player anwendbar ist
{
    public enum EnemyState { Moving, Hit, Dead } //usw. //evtl moving besser namen
    [HideInInspector] public EnemyState CurrentEnemyState = EnemyState.Moving;
    public bool ContactDamage;
    public int MaxHealth = 6;
    int CurrentHealth;
    int colorChangeCounter;
    public LayerMask layer_mask;
    Color originalColor;

    Coroutine EnemyKnockBack;
    Actor2D actor; // vllt reich der actor auf crawling enemy
    int CurrentHitPriority = 0;
    // Start is called before the first frame update
    void Start()
    {
        actor = GetComponent<Actor2D>();
        originalColor = GetComponent<SpriteRenderer>().color;
        CurrentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<CrawlingEnemy>() == null || CurrentEnemyState == EnemyState.Dead) //zwischen lösung für enemies ohne eigenes script 
        {
            if (actor.collision.above || actor.collision.below)
                actor.velocity = new Vector2(actor.velocity.x, 0);
            if (actor.collision.left || actor.collision.right)
                actor.velocity = new Vector2(0, actor.velocity.y);

            actor.velocity += Vector2.up * (-10 * Time.deltaTime);
            actor.velocity = new Vector2(actor.velocity.x, Mathf.Clamp(actor.velocity.y, -10, float.MaxValue));
        }
        if (CurrentEnemyState != EnemyState.Dead)
        {
            if(CurrentHealth <= 0)
            {
                actor.velocity = Vector2.zero;
                Destroy(gameObject, 2f); //despawn time //evtl länger?
                CurrentEnemyState = EnemyState.Dead;
            }
            if (ContactDamage) //enemy state attack
            {
                Vector2 ColliderBox = new Vector2(GetComponent<BoxCollider2D>().size.x * transform.localScale.x, GetComponent<BoxCollider2D>().size.y * transform.localScale.y);
                Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, ColliderBox, 0, layer_mask); //hitbox anpassen --> evtl etwas größer machen
                foreach (Collider2D collider in col)
                {
                    if (collider.CompareTag("Player"))
                    {
                        if (PlayerHook.CurrentPlayerState != PlayerHook.PlayerState.Disabled && collider.gameObject.GetComponent<PlayerCombat>().CurrentAttackState != PlayerCombat.AttackState.Smash) //collider.gameObject.GetComponent<PlayerCombat>().CurrentlyHit == false
                        {
                            collider.gameObject.GetComponent<PlayerCombat>().GetHit(transform, 30); //10 --> besseren fix finden
                            collider.gameObject.GetComponent<PlayerHook>().CancelHook();
                            if (GetComponent<Animator>() != null)
                                GetComponent<Animator>().SetTrigger("Attack"); //sollte auf jedenfall im anim script sein nur zum test hier
                        }
                    }
                }
            }
            if (CurrentEnemyState == EnemyState.Hit)
            {
                colorChangeCounter++;
                if (colorChangeCounter % 5 == 0)
                    GetComponent<SpriteRenderer>().color = Color.white;
                else
                    GetComponent<SpriteRenderer>().color = originalColor;
            }
        }
    }
    /*
    IEnumerator Despawn() //falls später evtl noch mehr passieren soll
    {

    }
    */
    public void GetHit(Transform _knockBackOrigin, float _KnockBackForce, int HitPriority) //bandaid fix for knockbackdirectino //jedesmal move prio checken und dann entscheiden ob man genockbacked wird oder nicht
    {
        if(CurrentEnemyState == EnemyState.Dead) //nochmal überprüfen ob das klappt
            return;
        //vllt die überprüfung ob der hit gilt hier rein machen
        if (CurrentEnemyState != EnemyState.Hit)
        {
            EnemyKnockBack = StartCoroutine(KnockBack(10, _knockBackOrigin, _KnockBackForce));
        }
        else if (CurrentEnemyState == EnemyState.Hit && HitPriority > CurrentHitPriority) //evtl reicht auch >= //ist das wirklich so ein guter ansatz?
        {
            StopCoroutine(EnemyKnockBack);
            EnemyKnockBack = StartCoroutine(KnockBack(10, _knockBackOrigin, _KnockBackForce));
        }
        CurrentHitPriority = HitPriority;
    }

    //besser machen und die schwerkraft usw alles mitberechnen --> evtl in ein anderes script //check collissions evtl auch woanders rein
    //was soll passieren wenn man den gegner / spieler in die wand knockt?
    IEnumerator KnockBack(float _repetissions, Transform _knockBackOrigin, float _KnockBackForce) //deactivate layer collission? //geht mit dem neuen system von freddie evtl nichtmerh //knockback direction hier festlegen
    {
        CurrentHealth--;
        CurrentEnemyState = EnemyState.Hit;
        for (int i = 0; i < _repetissions; i++)
        {
            float test = 1 - Mathf.Pow((i), 3) / 100; //warum?
            if (test < 0)
                test = 0;
            int AdditionalPosition = 0;
            if (Mathf.Abs(transform.position.x - _knockBackOrigin.position.x) < 0.15f) //KnockBacktolerance or so
                AdditionalPosition = 10;
            Vector2 KnockBackDirection = (transform.position - new Vector3(_knockBackOrigin.position.x + AdditionalPosition, _knockBackOrigin.position.y, _knockBackOrigin.position.z)).normalized;
            actor.velocity = KnockBackDirection * test * _KnockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee
            if (actor.collision.above || actor.collision.below)
                actor.velocity = new Vector2(actor.velocity.x, 0);
            if (actor.collision.left || actor.collision.right)
                actor.velocity = new Vector2(0, actor.velocity.y);

            yield return new WaitForSeconds(0.03f);
        }
        GetComponent<SpriteRenderer>().color = originalColor;
        colorChangeCounter = 0;
        CurrentHitPriority = 0;
        CurrentEnemyState = EnemyState.Moving;
    }
}
