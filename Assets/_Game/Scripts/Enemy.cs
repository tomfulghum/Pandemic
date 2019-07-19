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
    // Start is called before the first frame update
    void Start()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        if (ContactDamage)
        {
            Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, new Vector2(41 * transform.localScale.x, 41 * transform.localScale.y), 0, layer_mask); //hitbox anpassen
            foreach (Collider2D collider in col)
            {
                if (collider.CompareTag("Player"))
                {
                    if (collider.gameObject.GetComponent<PlayerCombat>().CurrentlyHit == false && collider.gameObject.GetComponent<PlayerCombat>().Smashing == false)
                    {
                        collider.gameObject.GetComponent<PlayerCombat>().GetHit(transform, collider.gameObject.GetComponent<Actor2D>().velocity.magnitude * 0.05f + 0.2f);
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

    public void GetHit(Transform _knockBackOrigin, float _strength) //bandaid fix for knockbackdirectino //jedesmal move prio checken und dann entscheiden ob man genockbacked wird oder nicht
    {
        StopAllCoroutines();
        StartCoroutine(KnockBack(10, _knockBackOrigin, _strength));
        CurrentlyHit = true;
        //EnemyFreeze = true
    }

    IEnumerator KnockBack(float _repetissions, Transform _knockBackOrigin, float _knockBackStrength) //deactivate layer collission? //geht mit dem neuen system von freddie evtl nichtmerh //knockback direction hier festlegen
    {
        Physics2D.IgnoreLayerCollision(10, 11, true); //geht wegen freddys script nichtmehr
        KnockBackActive = true;
        for (int i = 0; i < _repetissions; i++)
        {
            float test = 1 - Mathf.Pow((i), 3) / 100;
            if (test < 0)
            {
                test = 0;
            }
            //Debug.Log(test);
            if (_knockBackOrigin.transform.position.x > transform.position.x)
            {
                transform.position = new Vector2(transform.position.x - _knockBackStrength * test, transform.position.y);
            }
            else
            {
                transform.position = new Vector2(transform.position.x + _knockBackStrength * test, transform.position.y); //knock back velocity? für collission?
            }
            yield return new WaitForSeconds(0.03f);
        }
        KnockBackActive = false;
        //CurrentlyHit = false;
        GetComponent<SpriteRenderer>().color = originalColor;
        colorChangeCounter = 0;
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }
}
