using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueFlyerAnim : MonoBehaviour
{
    public GameObject Skull;
    Borb enemy; // in das richtige script um
    Animator anim;
    bool TriggeredDeath = false;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Borb>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.GetComponent<Enemy>().currentEnemyState == Enemy.EnemyState.Dead && TriggeredDeath == false)
        {
            anim.SetTrigger("Death");
            TriggeredDeath = true;
        }

        if (enemy.GetComponent<Enemy>().currentEnemyState == Enemy.EnemyState.Hit)
            anim.SetBool("Hit", true); //später evtl trigger
        else
            anim.SetBool("Hit", false);

        if (enemy.currentMovementState == Borb.MovementState.Chase)
            anim.SetFloat("AngryFlight", 1);
        else
            anim.SetFloat("AngryFlight", 0);

        if (enemy.currentMovementState == Borb.MovementState.Nosedive)
            anim.SetBool("Dive", true);
        else
            anim.SetBool("Dive", false);

        if (enemy.currentMovementState == Borb.MovementState.Dazed)
            anim.SetBool("Stuck", true);
        else
            anim.SetBool("Stuck", false);

        UpdateCollider(GetComponent<SpriteRenderer>().flipX);
        if (enemy.currentMovementDirection == Borb.MovementDirection.Left)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;
    }

    void Death()
    {
        GameObject PlagueFlyerSkull = Instantiate(Skull, transform.position, transform.rotation);
        if (GetComponent<SpriteRenderer>().flipX == true)
        {
            PlagueFlyerSkull.GetComponent<SpriteRenderer>().flipX = true;
            PlagueFlyerSkull.GetComponent<Collider2D>().offset = new Vector2(PlagueFlyerSkull.GetComponent<Collider2D>().offset.x * -1, PlagueFlyerSkull.GetComponent<Collider2D>().offset.y);
        }
        Destroy(gameObject);
    }

    void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
