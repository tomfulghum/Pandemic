using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPointVisualization : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private bool m_followPointer = true; //if false: visualizes time slow


    [SerializeField] private GameObject m_outerCircle = default;
    [SerializeField] private GameObject m_innerCircle = default;
    [SerializeField] private GameObject m_pointer = default;


    //**********************//
    //    Private Fields    //
    //**********************//

    private bool m_visualActive = false;

    float test = 10;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (m_visualActive && m_followPointer)
            AdjustInnerCircle();
        if (m_visualActive && m_followPointer == false)
            VisualizeTimeSlow();

    }


    //*************************//
    //    Private Functions    //
    //*************************//

    private void AdjustInnerCircle()
    {
        if (m_innerCircle.transform.rotation != m_pointer.transform.rotation)
        {
            float angle = Vector2.SignedAngle(m_innerCircle.transform.right, m_pointer.transform.right);
            float targetAngle = m_pointer.transform.eulerAngles.z;

            if (angle < 0)
            {
                m_innerCircle.transform.eulerAngles = new Vector3(0, 0, m_innerCircle.transform.eulerAngles.z - 2);
            }
            else
            {
                m_innerCircle.transform.eulerAngles = new Vector3(0, 0, m_innerCircle.transform.eulerAngles.z + 2);
            }

            if (Mathf.Abs(angle) < 1.5f)
            {
                m_innerCircle.transform.rotation = m_pointer.transform.rotation;
            }
        }
    }

    private void VisualizeTimeSlow()
    {
        float slowTest = Time.deltaTime / Time.timeScale * test * 100;
        //Debug.Log(slowTest);
        if (test > 0.1f)
            test *= 0.96f;
        else
            test = 0;
        m_innerCircle.transform.eulerAngles = new Vector3(0, 0, m_innerCircle.transform.eulerAngles.z + slowTest);
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

    public void SetObjectScale(float _hookRange) //erstmal nur ne zwischenlösung bis mir was besseres einfällt
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 1);
        transform.localScale *= _hookRange;
    }
}
