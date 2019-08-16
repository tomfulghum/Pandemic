using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_zOffset = -5f;

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

    public void VisualizeLine(Vector2 _startPos, Vector2 _endPos) //not working
    {
        if (m_lr.enabled == false)
            m_lr.enabled = true;

        Vector3 updatedStartPos = new Vector3(_startPos.x, _startPos.y, m_zOffset);
        Vector3 updatedEndPos = new Vector3(_endPos.x, _endPos.y, m_zOffset);
        m_lr.SetPosition(0, updatedStartPos);
        m_lr.SetPosition(1, updatedEndPos);
    }

    public void ActivateVisualization(bool _enabled)
    {
        m_lr.enabled = _enabled; 
        //delete all points?
    }
}
