using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private TextMeshProUGUI m_keysVisualization = default;
    [SerializeField] private int m_normalKeyCount = 0;

    //******************//
    //    Properties    //
    //******************//

    public int normalKeyCount
    {
        get { return m_normalKeyCount; }
    }

    private void UpdateKeyVisualization()
    {
        m_keysVisualization.text = "Keys: " + m_normalKeyCount;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void RemoveNormalKeys(int _count)
    {
        if (m_normalKeyCount >= _count) {
            m_normalKeyCount -= _count;
            UpdateKeyVisualization();
        }
    }

    public void AddNormalKey()
    {
        m_normalKeyCount++;
        UpdateKeyVisualization();
    }

    public void AddNormalKeys(int _count)
    {
        m_normalKeyCount += _count;
        UpdateKeyVisualization();
    }
}
