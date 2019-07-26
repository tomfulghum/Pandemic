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
        if (enemy.GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Dead && TriggeredDeath == false)
        {
            anim.SetTrigger("Death");
            TriggeredDeath = true;
        }
        if (enemy.CurrentMovementDirection == CrawlingEnemy.MovementDirection.Left)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;

        if (enemy.GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Hit)
        {
            anim.SetBool("Hit", true); //später evtl trigger
        }
        else //kann das zu fehlern führen?
        {
            anim.SetBool("Hit", false);
            if (enemy.Jumping)
                anim.SetBool("Jump", true);
            else
                anim.SetBool("Jump", false);
            if (enemy.CurrentMovementState == CrawlingEnemy.MovementState.Falling && enemy.Jumping == false)
                anim.SetBool("Falling", true);
            else
                anim.SetBool("Falling", false);
        }
    }
    //switch enemy state 

}
