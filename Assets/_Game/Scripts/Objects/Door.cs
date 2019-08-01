using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]

public class Door : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private bool m_open;

    //**********************//
    //    Private Fields    //
    //**********************//

    private SpriteRenderer m_sr;
    private BoxCollider2D m_coll;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_sr = GetComponent<SpriteRenderer>();
        m_coll = GetComponent<BoxCollider2D>();
    }

    private void OnValidate()
    {
        m_sr = GetComponent<SpriteRenderer>();
        m_coll = GetComponent<BoxCollider2D>();
        SetOpen(m_open);
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void SetOpen(bool _open)
    {
        m_open = _open;
        m_sr.enabled = !m_open;
        m_coll.enabled = !m_open;
    }

    public void Toggle()
    {
        SetOpen(!m_open);
    }
}
