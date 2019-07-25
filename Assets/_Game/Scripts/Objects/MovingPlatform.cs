using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private Rigidbody2D m_platform = default;
    [SerializeField] private bool m_running = true;
    [SerializeField] private float m_speed = 1f;
    [SerializeField] private float m_waitTime = 0f;
    [SerializeField] [Range(0, 2)] private float m_easeFactor = 0f;
    [SerializeField] private bool m_cyclic = false;
    [SerializeField] private Transform[] m_waypoints = default;
    
    //******************//
    //    Properties    //
    //******************//

    public bool running
    {
        get { return m_running; }
        set { m_running = value; }
    }

    //**********************//
    //    Private Fields    //
    //**********************//

    private int m_lastWaypoint = 0;
    private float m_progress = 0;
    private bool m_waiting = false;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    void FixedUpdate()
    {
        if (!m_waiting && m_running) {
            m_platform.velocity = CalculateVelocity();
        } else {
            m_platform.velocity = Vector2.zero;
        }
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private Vector2 CalculateVelocity()
    {
        m_lastWaypoint %= m_waypoints.Length;
        int nextWaypoint = (m_lastWaypoint + 1) % m_waypoints.Length;
        float distance = Vector3.Distance(m_waypoints[m_lastWaypoint].position, m_waypoints[nextWaypoint].position);

        m_progress += Time.fixedDeltaTime * m_speed / distance;
        m_progress = Mathf.Clamp01(m_progress);
        float easedProgress = Ease(m_progress);

        Vector2 newPosition = Vector2.Lerp(m_waypoints[m_lastWaypoint].position, m_waypoints[nextWaypoint].position, easedProgress);
        Vector2 velocity = (newPosition - m_platform.position) / Time.fixedDeltaTime;

        if (m_progress >= 1) {
            m_progress = 0;
            m_lastWaypoint++;
            if (!m_cyclic && m_lastWaypoint >= m_waypoints.Length - 1) {
                m_lastWaypoint = 0;
                System.Array.Reverse(m_waypoints);
            }

            if (m_waitTime > 0) {
                StartCoroutine(WaitCoroutine());
            }
        }

        return velocity;
    }

    private float Ease(float _x)
    {
        float a = m_easeFactor + 1;
        return Mathf.Pow(_x, a) / (Mathf.Pow(_x, a) + Mathf.Pow(1 - _x, a));
    }

    private IEnumerator WaitCoroutine()
    {
        m_waiting = true;
        yield return new WaitForSeconds(m_waitTime);
        m_waiting = false;
    }
}
