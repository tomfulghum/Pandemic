using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBackgroundParallax : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] int scrollSpeedPerSecond = 100;
    [SerializeField] int leftBound;
    [SerializeField] int rightBound;

    //**********************//
    //    Private Fields    //
    //**********************//

    private RectTransform m_image;
    private Vector3 maxVector;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_image = GetComponent<RectTransform>();
    }

    void Update()
    {
        m_image.anchoredPosition3D += new Vector3(scrollSpeedPerSecond * Time.deltaTime, 0, 0);

        if (m_image.anchoredPosition3D.x > rightBound) {
            m_image.anchoredPosition3D = new Vector3(leftBound, 0, 0);
        }
    }
}
