using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBrawler : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//
    public enum MovementState
    {
        Decide,
        Idle,
        Move,
        Attack,
        Stuck,
        Block,
        Chase
    }
    public enum MovementDirection
    {
        None,
        Left,
        Right
    }

    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_movementSpeed = 1f;
    [SerializeField] private float m_blockRange = 3f;
    [SerializeField] private LayerMask m_lethalObjects = default;

    //******************//
    //    Properties    //
    //******************//

    public MovementState currentMovementState
    {
        get { return m_currentMovementState; }
    }

    public MovementDirection currentMovementDirection
    {
        get { return m_currentMovementDirection; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private MovementState m_currentMovementState = MovementState.Decide;
    private MovementDirection m_currentMovementDirection = MovementDirection.Right;


    private Transform m_objectToChase;

    private Enemy m_enemy;
    private Rigidbody2D m_rb;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Start()
    {
        m_enemy = GetComponent<Enemy>();
        m_rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void SetMovementState()
    {
        if(LethalObjectInRange())
        {
            m_currentMovementState = MovementState.Block;
        } else
        {
            m_currentMovementState = MovementState.Move;
        }
    }

    private bool LethalObjectInRange()
    {
        Collider2D[] hookPointsInRange = Physics2D.OverlapCircleAll(transform.position, m_blockRange, m_lethalObjects);
        foreach(Collider2D col in hookPointsInRange)
        {
            if(col.CompareTag("Throwable")) //&& col.GetComponent<ThrowableObject>(). ... == thrown
            {
                m_enemy.invincible = true;
                return true;
            }
        }
        m_enemy.invincible = false;
        return false;
    }
}
