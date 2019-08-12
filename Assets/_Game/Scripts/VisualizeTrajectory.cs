using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//könnte evtl noch ein problem mit dem destroy/childcount geben --> evtl wäre object pooling besser?
public class VisualizeTrajectory : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] [Range(0, 100)] private int m_numOfVisualDots = 50;
    [SerializeField] private float m_timeBetweenDots = 0.07f; //besseren namen finden
    [SerializeField] private GameObject m_dotPrefab = null;
    [SerializeField] private LayerMask m_ObjectCollission = default; //womit "kollidiert" das flugobject 

    //**********************//
    //    Private Fields    //
    //**********************//

    private List<GameObject> m_dots = new List<GameObject>();

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    //num of visual dots ersetzen durch dichte der punkte --> oder zeitabstand?
    void Start()
    {
    }

    void Update()
    {

    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private Vector2 CalculatePosition(float elapsedTime, Vector2 _launchVelocity, Vector2 _initialPosition, Vector2 _gravity)
    {
        return _gravity * elapsedTime * elapsedTime * 0.5f + _launchVelocity * elapsedTime + _initialPosition;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void VisualizeDots(Vector2 _startPosition, Vector2 _launchVelocity) //gravity sollte irgendwoanders gesetzt werden --> am besten von dem object das geworfen wird
    {
        RemoveVisualDots();

        bool hitSmth = false;
        float throwTime = 0f;
        int dotCount = 0;
        while (hitSmth == false && dotCount < m_numOfVisualDots) { //|| currentnum of dots > max num of visualdots --> als sicherung --> da funktioniert noch was nicht sogut
            Vector2 startPosition = CalculatePosition(throwTime, _launchVelocity, _startPosition, Physics2D.gravity);
            throwTime += m_timeBetweenDots; //dafür variable aus dem editor nehmen 
            Vector2 targetPosition = CalculatePosition(throwTime, _launchVelocity, _startPosition, Physics2D.gravity);
            float raycastLength = (targetPosition - startPosition).magnitude;
            RaycastHit2D hit = Physics2D.Raycast(startPosition, (targetPosition - startPosition), raycastLength, m_ObjectCollission); //vllt anstatt 1 irgendwas ausrechnen?

            GameObject trajectoryDot = Instantiate(m_dotPrefab);
            trajectoryDot.transform.position = startPosition;
            dotCount++;
            m_dots.Add(trajectoryDot);

            if (hit.collider != null) {
                hitSmth = true;
            }
        }
    }

    public void RemoveVisualDots()
    {
        foreach (var dot in m_dots) {
            if (dot) {
                Destroy(dot);
            }
        }

        m_dots.Clear(); 
    }
}
