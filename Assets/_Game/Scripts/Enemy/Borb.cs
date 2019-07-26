using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borb : MonoBehaviour
{
    public enum MovementState { Decide, Move, FlyUp, Nosedive, Chase }
    public enum MovementDirection {None, Left, Right }

    [HideInInspector] public MovementState CurrentMovementState = MovementState.Decide; //vllt am anfang auf decide
    [HideInInspector] public MovementDirection CurrentMovementDirection = MovementDirection.Left;

    public float ChaseRange = 3f;
    public float MovementSpeed = 3f;
    int DirectionCounter;
    float flightHeight; //bei knockback zurück auf die flight height

    Actor2D actor;
    Enemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        flightHeight = transform.position.y;
        actor = GetComponent<Actor2D>();
        enemy = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.CurrentEnemyState == Enemy.EnemyState.Moving)
        {
            CheckFlightHeight();
            DirectionCounter--;
            if (DirectionCounter < 0)
                ChangeDirection();
            if (CurrentMovementDirection == MovementDirection.Right)
                actor.velocity = Vector2.right * MovementSpeed;
            else
                actor.velocity = Vector2.left * MovementSpeed;
        } 
    }

    void ChangeDirection()
    {
        if (CurrentMovementDirection == MovementDirection.Left)
            CurrentMovementDirection = MovementDirection.Right;
        else
            CurrentMovementDirection = MovementDirection.Left;
        DirectionCounter = 150 + Random.Range(0, 100);
    }

    void CheckFlightHeight() //später noch besser machen
    {
        if(transform.position.y != flightHeight)
        {
            if (transform.position.y < flightHeight)
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x, transform.position.y - 0.05f, transform.position.z);

            if (Mathf.Abs(transform.position.y - flightHeight) < 0.1f)
                transform.position = new Vector3(transform.position.x, flightHeight, transform.position.z);
        }
    }
}
