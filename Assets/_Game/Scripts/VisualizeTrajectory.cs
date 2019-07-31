using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeTrajectory : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] [Range(0, 100)] private int m_numOfVisualDots = 50;
    [SerializeField] private float m_timeBetweenDots = 0.07f; //besseren namen finden
    [SerializeField] private GameObject m_dotPrefab = null;
    [SerializeField] private LayerMask m_layerMask = default; //womit "kollidiert" das flugobject

    //**********************//
    //    Private Fields    //
    //**********************//

    private GameObject m_dotParent;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    //num of visual dots ersetzen durch dichte der punkte --> oder zeitabstand?
    void Start()
    {
        m_dotParent = new GameObject("Parent Dot");
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
        m_dotParent.transform.position = _startPosition;
        //float TravelTime = CalculateTravelTime( _startPosition,  _launchVelocity, new Vector2(0, -_gravity));
        //float TravelTimeForEachDot = TravelTime / NumOfVisualDots;
        bool hitSmth = false;
        float throwTime = 0f;
        while (hitSmth == false && m_dotParent.transform.childCount < m_numOfVisualDots) { //|| currentnum of dots > max num of visualdots --> als sicherung --> da funktioniert noch was nicht sogut
            Vector2 startPosition = CalculatePosition(throwTime, _launchVelocity, m_dotParent.transform.position, Physics2D.gravity);
            throwTime += m_timeBetweenDots; //dafür variable aus dem editor nehmen 
            Vector2 targetPosition = CalculatePosition(throwTime, _launchVelocity, m_dotParent.transform.position, Physics2D.gravity);
            float raycastLength = (targetPosition - startPosition).magnitude;
            RaycastHit2D hit = Physics2D.Raycast(startPosition, (targetPosition - startPosition), raycastLength, m_layerMask); //vllt anstatt 1 irgendwas ausrechnen?
            if (hit.collider == null) {
                GameObject trajectoryDot = Instantiate(m_dotPrefab);
                trajectoryDot.transform.SetParent(m_dotParent.transform);
                trajectoryDot.transform.position = startPosition;
            } else {
                hitSmth = true;
            }
        }
        /*
        for (int i = 0; i < NumOfVisualDots; i++)
        {
            GameObject trajectoryDot = Instantiate(DotPrefab);
            trajectoryDot.transform.SetParent(DotParent.transform);
            trajectoryDot.transform.position = CalculatePosition(0.05f * i, _launchVelocity, DotParent.transform.position, new Vector2(0, -_gravity));

        }
        */
    }

    public void RemoveVisualDots()
    {
        foreach (Transform child in m_dotParent.transform) { //vllt später jedesmal wieder die gleichen objects benutzen -->object pooling --> check if num of visual dots == num of childs ansonsten neue erstellen usw.
            Destroy(child.gameObject);
        }
    }
}
