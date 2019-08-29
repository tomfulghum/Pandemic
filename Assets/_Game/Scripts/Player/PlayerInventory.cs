using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        set { m_normalKeyCount = value; }
    }
}
