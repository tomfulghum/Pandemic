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
        if (enemy.GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Dead && TriggeredDeath == false)
        {
            anim.SetTrigger("Death");
            TriggeredDeath = true;
        }
        if (enemy.CurrentMovementDirection == Borb.MovementDirection.Left)
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
}
