using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaguePeasantAnim : MonoBehaviour
{
    PlaguePeasant enemy;
    Animator anim;
    Actor2D actor;
    bool TriggeredDeath = false;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        enemy = GetComponent<PlaguePeasant>();
        actor = GetComponent<Actor2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Dead && TriggeredDeath == false)
        {
            anim.SetTrigger("Death");
            TriggeredDeath = true;
        }
        UpdateCollider(GetComponent<SpriteRenderer>().flipX);
        if (enemy.CurrentMovementDirection == PlaguePeasant.MovementDirection.Left)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;

        if (enemy.CurrentMovementState == PlaguePeasant.MovementState.Move) //ist das mit dem float so eine gute idee?
            anim.SetBool("Moving", true);
        else
            anim.SetBool("Moving", false);

        if (enemy.CurrentMovementState == PlaguePeasant.MovementState.Sit) //ist das mit dem float so eine gute idee?
            anim.SetBool("Sitting", true);
        else
            anim.SetBool("Sitting", false);

        if (GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Hit)
            anim.SetBool("Hit", true); //später evtl trigger
        else
            anim.SetBool("Hit", false);
    }

    void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
