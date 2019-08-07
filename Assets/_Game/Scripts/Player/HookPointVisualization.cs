using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPointVisualization : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private GameObject m_outerCircle = default;
    [SerializeField] private GameObject m_innerCircle = default;
    [SerializeField] private GameObject m_pointer = default;


    //**********************//
    //    Private Fields    //
    //**********************//

    private bool m_visualActive = false;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (m_visualActive)
            AdjustInnerCircle();
    }


    //*************************//
    //    Private Functions    //
    //*************************//

    private void AdjustInnerCircle()
    {
        if (m_innerCircle.transform.rotation != m_pointer.transform.rotation)
        {
            // m_innerCircle.transform.rotation = Quaternion.Lerp(m_innerCircle.transform.rotation, Quaternion.FromToRotation(m_innerCircle.transform.position, m_pointer.transform.position - m_innerCircle.transform.position), Time.deltaTime * 2);
            // m_innerCircle.transform.rotation = Quaternion.Lerp(m_innerCircle.transform.rotation, m_pointer.transform.rotation, Time.time * 1);

            float angle = Vector2.Angle(m_innerCircle.transform.right, m_pointer.transform.right);
            m_innerCircle.transform.rotation = Quaternion.Lerp(m_innerCircle.transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime / Time.timeScale * 2);

            //Vector3 vectorToTarget = m_innerCircle.transform.position - m_pointer.transform.position;
            //Debug.Log("vector to target: " + vectorToTarget);
            ////float angle = Vector2.Angle(m_innerCircle.transform.right, m_pointer.transform.right);
            //Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            //Debug.Log("q: " + q);
            //m_innerCircle.transform.rotation = Quaternion.Slerp(m_innerCircle.transform.rotation, q, Time.deltaTime / Time.timeScale * 2);

            //transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.AngleAxis(angle - 90.0f, Vector3.forward), Time.deltaTime * 2);

            Debug.Log("here");
        }
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void ActivateVisuals(bool _active)
    {
        m_outerCircle.SetActive(_active);
        m_innerCircle.SetActive(_active);
        m_pointer.SetActive(_active);
        m_visualActive = _active;
    }

    public void SetPointerDirection(Vector2 _direction)
    {
        var angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        m_pointer.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
