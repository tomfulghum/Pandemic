using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaguePeasantAnim : MonoBehaviour
{
    CrawlingEnemy enemy;
    Animator anim;
    bool TriggeredDeath = false;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        if (enemy != null)
            enemy = GetComponent<CrawlingEnemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<Enemy>().CurrentEnemyState == Enemy.EnemyState.Dead && TriggeredDeath == false)
        {
            anim.SetTrigger("Death");
            TriggeredDeath = true;
        }
        if (enemy != null)
        {
            UpdateCollider(GetComponent<SpriteRenderer>().flipX);
            if (enemy.CurrentMovementDirection == CrawlingEnemy.MovementDirection.Left)
                GetComponent<SpriteRenderer>().flipX = false;
            else
                GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    void UpdateCollider(bool _flipX) //könnte problematisch werden wenn der offset am anfang schon negativ ist
    {
        if (_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == 1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
        else if (!_flipX && Mathf.Sign(GetComponent<Collider2D>().offset.x) == -1)
            GetComponent<Collider2D>().offset = new Vector2(GetComponent<Collider2D>().offset.x * -1, GetComponent<Collider2D>().offset.y);
    }
}
