using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private int m_normalKeyCount = 0;

    //******************//
    //    Properties    //
    //******************//

    public int normalKeyCount
    {
        get { return m_normalKeyCount; }
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void RemoveNormalKeys(int _count)
    {
        if (m_normalKeyCount >= _count) {
            m_normalKeyCount -= _count;
        }
    }

    public void AddNormalKey()
    {
        m_normalKeyCount++;
    }

    public void AddNormalKeys(int _count)
    {
        m_normalKeyCount += _count;
    }
}
