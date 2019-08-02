using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_force = 10f;
    [SerializeField] private float m_lifetime = 5f;
    [SerializeField] private LayerMask m_collidingLayers = default;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, m_lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 colliderBox = new Vector2(GetComponent<BoxCollider2D>().size.x * transform.localScale.x, GetComponent<BoxCollider2D>().size.y * transform.localScale.y);
        Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, colliderBox, 0, m_collidingLayers);
        foreach(Collider2D collider in col)
        {
            if(collider.CompareTag("Player"))
            {
                if (PlayerHook.CurrentPlayerState != PlayerHook.PlayerState.Disabled && collider.gameObject.GetComponent<PlayerCombat>().currentAttackState != PlayerCombat.AttackState.Smash)
                { 
                    collider.gameObject.GetComponent<PlayerCombat>().GetHit(transform.position, m_force); 
                    collider.gameObject.GetComponent<PlayerHook>().CancelHook();
                }
            }
        }
        if (col.Length != 0)
            Destroy(gameObject);
    }
}
