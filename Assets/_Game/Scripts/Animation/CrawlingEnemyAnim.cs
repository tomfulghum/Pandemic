using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//überlegen wie das ganze mit falling und landing oder jump start usw zusammenpasst
public class CrawlingEnemyAnim : MonoBehaviour
{
    CrawlingEnemy enemy;
    Animator anim;

    bool TriggeredDeath;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<CrawlingEnemy>();
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
        //UpdateCollider(GetComponent<SpriteRenderer>().flipX);
        if (enemy.currentMovementDirection == CrawlingEnemy.MovementDirection.Left)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;

        if (enemy.GetComponent<Enemy>().currentEnemyState == Enemy.EnemyState.Hit)
        {
            anim.SetBool("Hit", true); //später evtl trigger
        }
        else //kann das zu fehlern führen?
        {
            anim.SetBool("Hit", false);
            if (enemy.jumping)
                anim.SetBool("Jump", true);
            else
                anim.SetBool("Jump", false);
            if (enemy.currentMovementState == CrawlingEnemy.MovementState.Falling && enemy.jumping == false)
                anim.SetBool("Falling", true);
            else
                anim.SetBool("Falling", false);
        }
    }
    //switch enemy state 

    void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
