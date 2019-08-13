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
        set 
        {
            m_normalKeyCount = value;
            UpdateKeyVisualization();
        }
    }

    private void UpdateKeyVisualization()
    {
        m_keysVisualization.text = "Keys: " + m_normalKeyCount;
    }
}
