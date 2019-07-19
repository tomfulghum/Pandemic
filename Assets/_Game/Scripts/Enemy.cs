using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enemy in eine elternklasse umwandeln die für alle kind enemys deren movementpattern funktion im update aufruft
public class Enemy : MonoBehaviour //vllt anstatt enemy ein allgemeines script schreiben was auch für den player anwendbar ist
{
    public enum EnemyState { Moving, Hit } //usw.
    public EnemyState CurrentEnemyState = EnemyState.Moving;
    public bool CurrentlyHit;
    public bool ContactDamage;
    bool KnockBackActive;
    int colorChangeCounter;
    public LayerMask layer_mask;
    Color originalColor;

    Actor2D actor; // vllt reich der actor auf crawling enemy
    // Start is called before the first frame update
    void Start()
    {
        actor = GetComponent<Actor2D>();
        originalColor = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<CrawlingEnemy>() == null) //zwischen lösung für enemies ohne eigenes script
        {
            if (actor.collision.above || actor.collision.below)
                actor.velocity = new Vector2(actor.velocity.x, 0);
            if (actor.collision.left || actor.collision.right)
                actor.velocity = new Vector2(0, actor.velocity.y);

            actor.velocity += Vector2.up * (-10 * Time.deltaTime);
            actor.velocity = new Vector2(actor.velocity.x, Mathf.Clamp(actor.velocity.y, -10, float.MaxValue));
        }
        if (ContactDamage)
        {
            Vector2 ColliderBox = new Vector2(GetComponent<BoxCollider2D>().size.x * transform.localScale.x, GetComponent<BoxCollider2D>().size.y * transform.localScale.y);
            Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, ColliderBox, 0, layer_mask); //hitbox anpassen --> evtl etwas größer machen
            foreach (Collider2D collider in col)
            {
                if (collider.CompareTag("Player"))
                {
                    if (collider.gameObject.GetComponent<PlayerCombat>().CurrentlyHit == false && collider.gameObject.GetComponent<PlayerCombat>().Smashing == false)
                    {
                        collider.gameObject.GetComponent<PlayerCombat>().GetHit(transform, collider.gameObject.GetComponent<Actor2D>().velocity.magnitude * 0.3f + 10);
                        collider.gameObject.GetComponent<PlayerHook>().CancelHook();
                    }
                }
            }
        }
        if (KnockBackActive)
        {
            CurrentEnemyState = EnemyState.Hit;
            colorChangeCounter++;
            if (colorChangeCounter % 5 == 0)
            {
                GetComponent<SpriteRenderer>().color = Color.white;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = originalColor;
            }
        }
        else
        {
            CurrentEnemyState = EnemyState.Moving;
        }
    }

    public void GetHit(Transform _knockBackOrigin, float _KnockBackForce) //bandaid fix for knockbackdirectino //jedesmal move prio checken und dann entscheiden ob man genockbacked wird oder nicht
    {
        //vllt die überprüfung ob der hit gilt hier rein machen
        StopAllCoroutines();
        StartCoroutine(KnockBack(10, _knockBackOrigin, _KnockBackForce));
        CurrentlyHit = true;
        //EnemyFreeze = true
    }

    //besser machen und die schwerkraft usw alles mitberechnen --> evtl in ein anderes script //check collissions evtl auch woanders rein
    //was soll passieren wenn man den gegner / spieler in die wand knockt?
    IEnumerator KnockBack(float _repetissions, Transform _knockBackOrigin, float _KnockBackForce) //deactivate layer collission? //geht mit dem neuen system von freddie evtl nichtmerh //knockback direction hier festlegen
    {
        //Physics2D.IgnoreLayerCollision(10, 11, true); //geht wegen freddys script nichtmehr
        KnockBackActive = true;
        for (int i = 0; i < _repetissions; i++)
        {
            float test = 1 - Mathf.Pow((i), 3) / 100; //warum?
            if (test < 0)
                test = 0;
            //Debug.Log(test);
            Vector2 KnockBackDirection = (transform.position - _knockBackOrigin.position).normalized;
            actor.velocity = KnockBackDirection * test * _KnockBackForce; //currently no gravity? --> wahrscheinlich ne gute idee
            if (actor.collision.above || actor.collision.below)
                actor.velocity = new Vector2(actor.velocity.x, 0);
            if (actor.collision.left || actor.collision.right)
                actor.velocity = new Vector2(0, actor.velocity.y);

            yield return new WaitForSeconds(0.03f);
        }
        KnockBackActive = false;
        //CurrentlyHit = false;
        GetComponent<SpriteRenderer>().color = originalColor;
        colorChangeCounter = 0;
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }
}
