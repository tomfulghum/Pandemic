using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private MovingObject platform = default;
    [SerializeField] private float speed = 1f;
    [SerializeField] private bool cyclic = false;
    [SerializeField] private bool smooth = false;
    [SerializeField] private Transform[] waypoints = default;

    //**********************//
    //    Private Fields    //
    //**********************//

    private int lastWaypoint = 0;
    private float progress = 0;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void Update()
    {
        platform.Translate(CalculateDeltaPosition());
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private Vector2 CalculateDeltaPosition()
    {
        int nextWaypoint = lastWaypoint + 1;
        float distance = Vector3.Distance(waypoints[lastWaypoint].position, waypoints[nextWaypoint].position);
        progress += Time.deltaTime * speed / distance;
        if (progress >= 1) {
            progress = 0;
            lastWaypoint++;
            if (lastWaypoint >= waypoints.Length - 1) {
                lastWaypoint = 0;
                System.Array.Reverse(waypoints);
            }
        }

        Vector3 newPosition = Vector3.Lerp(waypoints[lastWaypoint].position, waypoints[nextWaypoint].position, progress);
        return (newPosition - platform.transform.position);
    }
}
