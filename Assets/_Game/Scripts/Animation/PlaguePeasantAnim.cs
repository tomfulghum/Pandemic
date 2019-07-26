using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaguePeasantAnim : MonoBehaviour
{
    CrawlingEnemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<CrawlingEnemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.CurrentMovementDirection == CrawlingEnemy.MovementDirection.Left)
            GetComponent<SpriteRenderer>().flipX = false;
        else
            GetComponent<SpriteRenderer>().flipX = true;
    }
}
