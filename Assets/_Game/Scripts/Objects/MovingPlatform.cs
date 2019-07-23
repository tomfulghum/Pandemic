using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private MovingObject platform = default;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float waitTime = 0f;
    [SerializeField] [Range(0, 2)] private float easeFactor = 0f;
    [SerializeField] private bool cyclic = false;
    [SerializeField] private Transform[] waypoints = default;
    
    //******************//
    //    Properties    //
    //******************//

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private int lastWaypoint = 0;
    private float progress = 0;
    private bool waiting = false;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Update()
    {
        if (!waiting) {
            platform.Translate(CalculateDeltaPosition());
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private Vector2 CalculateDeltaPosition()
    {
        lastWaypoint %= waypoints.Length;
        int nextWaypoint = (lastWaypoint + 1) % waypoints.Length;
        float distance = Vector3.Distance(waypoints[lastWaypoint].position, waypoints[nextWaypoint].position);

        progress += Time.deltaTime * speed / distance;
        progress = Mathf.Clamp01(progress);
        float easedProgress = Ease(progress);

        Vector3 newPosition = Vector3.Lerp(waypoints[lastWaypoint].position, waypoints[nextWaypoint].position, easedProgress);

        if (progress >= 1) {
            progress = 0;
            lastWaypoint++;
            if (!cyclic && lastWaypoint >= waypoints.Length - 1) {
                lastWaypoint = 0;
                System.Array.Reverse(waypoints);
            }

            if (waitTime > 0) {
                StartCoroutine(WaitCoroutine());
            }
        }

        return (newPosition - platform.transform.position);
    }

    private float Ease(float _x)
    {
        float a = easeFactor + 1;
        return Mathf.Pow(_x, a) / (Mathf.Pow(_x, a) + Mathf.Pow(1 - _x, a));
    }

    private IEnumerator WaitCoroutine()
    {
        waiting = true;
        yield return new WaitForSeconds(waitTime);
        waiting = false;
    }
}
