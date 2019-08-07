using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] [Range(0, 2)] private float m_speedMultiplier = 0.5f; //später per object type einstellen
    [SerializeField] private float m_knockBackForce = 10f;
    [SerializeField] private float m_lifetime = 5f;
    [SerializeField] private LayerMask m_collidingLayers = default;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    // Start is called before the first frame update
    private void Start()
    {
        Destroy(gameObject, m_lifetime);
    }

    // Update is called once per frame
    private void Update()
    {
        Vector2 colliderBox = new Vector2(GetComponent<BoxCollider2D>().size.x * transform.localScale.x, GetComponent<BoxCollider2D>().size.y * transform.localScale.y);
        Collider2D[] col = Physics2D.OverlapBoxAll(transform.position, colliderBox, 0, m_collidingLayers);
        foreach (Collider2D collider in col)
        {
            if (collider.CompareTag("Player"))
            {
                if (PlayerHook.CurrentPlayerState != PlayerHook.PlayerState.Disabled && collider.gameObject.GetComponent<PlayerCombat>().currentAttackState != PlayerCombat.AttackState.Smash)
                {
                    collider.gameObject.GetComponent<PlayerCombat>().GetHit(transform.position, m_knockBackForce);
                    collider.gameObject.GetComponent<PlayerHook>().CancelHook();
                }
            }
        }
        if (col.Length != 0)
            Destroy(gameObject);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void ApplySpeedMultiplier()
    {
        GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity * m_speedMultiplier;
        GetComponent<Rigidbody2D>().gravityScale *= Mathf.Pow(m_speedMultiplier, 2);
    }

}
