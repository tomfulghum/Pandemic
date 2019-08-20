using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_knockBackForce = 10f;
    [SerializeField] private float m_lifetime = 5f;
    [SerializeField] private float m_rotationOffset = 0;

    //******************//
    //    Properties    //
    //******************//
    public float rotationOffset
    {
        get { return m_rotationOffset; }
        set { m_rotationOffset = value; }
    }

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
            transform.rotation = Quaternion.AngleAxis(angle + m_rotationOffset, Vector3.forward);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_isLethal && collision.CompareTag("Player"))
        {
            m_anim.SetFloat("HitPlayer", 1f);
            PlayerAnim pa = collision.GetComponent<PlayerAnim>();
            if (pa.currentPlayerState != PlayerAnim.PlayerState.Disabled && pa.currentPlayerState != PlayerAnim.PlayerState.Invincible)
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

    public void ApplySpeedMultiplier(float _speedMultiplier)
    {
        GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity * _speedMultiplier;
        GetComponent<Rigidbody2D>().gravityScale *= Mathf.Pow(_speedMultiplier, 2);
    }

}
