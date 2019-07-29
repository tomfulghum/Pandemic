using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float Lifetime = 5f;
    public LayerMask CollidingLayers;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, Lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 ColliderBox = new Vector2(GetComponent<BoxCollider2D>().size.x * transform.localScale.x, GetComponent<BoxCollider2D>().size.y * transform.localScale.y);
        Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, ColliderBox, 0, CollidingLayers);
        if (col.Length != 0)
            Destroy(gameObject);
    }
}
