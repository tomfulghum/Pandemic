using UnityEngine;

[RequireComponent(typeof(Actor2D))]

public class PlayerTest : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private float gravity = -20f;

    //**********************//
    //    Private Fields    //
    //**********************//

    private Actor2D actor;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        actor = GetComponent<Actor2D>();
    }

    private void Update()
    {
        actor.velocity += gravity * Vector2.up * Time.deltaTime;

        float x = Input.GetAxisRaw("Horizontal");
        actor.velocity = new Vector2(x * 8f, actor.velocity.y);
    }
}
