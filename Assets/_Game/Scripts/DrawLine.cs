using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    //**********************//
    //    Private Fields    //
    //**********************//

    private LineRenderer m_lr = default;

    // Start is called before the first frame update
    void Start()
    {
        m_lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void VisualizeLine(Vector3 _startPos, Vector3 _endPos) //not working
    {
        m_lr.SetPosition(0, _startPos);
        m_lr.SetPosition(1, _endPos);
    }
}
