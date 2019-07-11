using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingEnemy : MonoBehaviour
{
    public float MovementSpeed;
    public float ChaseRadius;
    bool movingLeft;
    int counter;
    int maxcounter;
    bool chasingPlayer;
    // Start is called before the first frame update
    void Start()
    {
        maxcounter = 200;
    }

    // Update is called once per frame
    void Update()
    {
        ChasePlayer();
        Debug.Log(chasingPlayer);
        if (!chasingPlayer)
        {
            counter++;
            if (CheckGroundAhead() == false || counter % maxcounter == 0)
            {
                maxcounter = 200 + Random.Range(0, 200);
                counter = 0;
                movingLeft = !movingLeft;
            }
        }
        if (movingLeft)
        {
            transform.Translate(Vector3.left * Time.deltaTime * MovementSpeed);
        }

        if (!movingLeft)
        {
            transform.Translate(Vector3.right * Time.deltaTime * MovementSpeed);
        }
    }

    void ChasePlayer()
    {
        Collider2D[] ColliderInRange = Physics2D.OverlapCircleAll(transform.position, ChaseRadius);
        for (int i = 0; i < ColliderInRange.Length; i++)
        {
            Debug.Log("test2");
            if (ColliderInRange[i].CompareTag("Player"))
            {
                Debug.Log("test");
                chasingPlayer = true;
                if(ColliderInRange[i].transform.position.x > transform.position.x)
                {
                    movingLeft = false;
                } else
                {
                    movingLeft = true;
                }
                return;
            }
        }
        chasingPlayer = false;
    }

    bool CheckGroundAhead()
    {
        RaycastHit2D hit;
        if (movingLeft)
        {
            hit = Physics2D.Raycast(transform.position + Vector3.left, -Vector2.up, 1);
        }
        else
        {
            hit = Physics2D.Raycast(transform.position + Vector3.right, -Vector2.up, 1);
        }
        if (hit.collider != null)
        {
            return true;
        }
        return false;
    }
}
