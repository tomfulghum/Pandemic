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

    //**********************//
    //    Private Fields    //
    //**********************//

    [SerializeField] private bool m_isLethal = true;

    [SerializeField] private Rigidbody2D m_rb;
    [SerializeField] private Animator m_anim;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    // Start is called before the first frame update
    private void Start()
    {
        Destroy(gameObject, m_lifetime);
        m_rb = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_isLethal)
        {
            CorrectRotation();
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void CorrectRotation()
    {
        Vector2 moveDirection = m_rb.velocity;
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_isLethal && collision.CompareTag("Player"))
        {
            m_anim.SetFloat("HitPlayer", 1f);
            if (PlayerHook.CurrentPlayerState != PlayerHook.PlayerState.Disabled && PlayerHook.CurrentPlayerState != PlayerHook.PlayerState.Invincible)
            {
                collision.GetComponent<PlayerCombat>().GetHit(transform.position, m_knockBackForce);
            }
        }
        m_isLethal = false;
        m_rb.isKinematic = true;
        transform.rotation = Quaternion.identity;
        m_rb.velocity = Vector2.zero;
        m_anim.SetTrigger("Destroy");
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
