using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeTrajectory : MonoBehaviour
{
    [Range(0,100)] public int NumOfVisualDots;
    public float TimeBetweenDots = 0.07f; //besseren namen finden
    public GameObject DotPrefab;
    public LayerMask layer_mask;
    GameObject DotParent;
    //num of visual dots ersetzen durch dichte der punkte --> oder zeitabstand?
    // Start is called before the first frame update
    void Start()
    {
        DotParent = new GameObject("Parent Dot"); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void VisualizeDots(Vector2 _startPosition, Vector2 _launchVelocity ,float _gravity) //gravity sollte irgendwoanders gesetzt werden --> am besten von dem object das geworfen wird
    {
        RemoveVisualeDots();
        DotParent.transform.position = _startPosition;
        //float TravelTime = CalculateTravelTime( _startPosition,  _launchVelocity, new Vector2(0, -_gravity));
        //float TravelTimeForEachDot = TravelTime / NumOfVisualDots;
        bool HitSmth = false;
        float ThrowTime = 0f;
        while(HitSmth == false && DotParent.transform.childCount < NumOfVisualDots) //|| currentnum of dots > max num of visualdots --> als sicherung --> da funktioniert noch was nicht sogut
        {
            Vector2 StartPosition = CalculatePosition(ThrowTime, _launchVelocity, DotParent.transform.position, new Vector2(0, -_gravity));
            ThrowTime += TimeBetweenDots; //dafür variable aus dem editor nehmen 
            Vector2 TargetPosition = CalculatePosition(ThrowTime, _launchVelocity, DotParent.transform.position, new Vector2(0, -_gravity));
            float RaycastLenght = (TargetPosition - StartPosition).magnitude;
            RaycastHit2D hit = Physics2D.Raycast(StartPosition, (TargetPosition - StartPosition), RaycastLenght, layer_mask); //vllt anstatt 1 irgendwas ausrechnen?
            if(hit.collider == null)
            {
                GameObject trajectoryDot = Instantiate(DotPrefab);
                trajectoryDot.transform.SetParent(DotParent.transform);
                trajectoryDot.transform.position = StartPosition;
            } else
            {
                HitSmth = true;
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

    public void RemoveVisualeDots()
    {
        foreach (Transform child in DotParent.transform) //vllt später jedesmal wieder die gleichen objects benutzen -->object pooling --> check if num of visual dots == num of childs ansonsten neue erstellen usw.
        {
            Destroy(child.gameObject);
        }
    }

    Vector2 CalculatePosition(float elapsedTime, Vector2 _launchVelocity, Vector2 _initialPosition, Vector2 _gravity)
    {
        return _gravity * elapsedTime * elapsedTime * 0.5f + _launchVelocity * elapsedTime + _initialPosition;
    }
}
