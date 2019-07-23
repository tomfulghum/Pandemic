using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BackgroundParallax : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float m_parallaxFactor = 0f;

    //**********************//
    //    Private Fields    //
    //**********************//

    private List<BackgroundParallaxElement> m_elements = new List<BackgroundParallaxElement>();
    private CinemachineBrain m_mainCamera;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        m_mainCamera = Camera.main.GetComponent<CinemachineBrain>();

        foreach (Transform child in transform) {
            var element = child.GetComponent<BackgroundParallaxElement>();
            if (element) {
                m_elements.Add(element);
            }
        }
    }

    private void Update()
    {
        CinemachineVirtualCamera activeCamera = (CinemachineVirtualCamera)m_mainCamera.ActiveVirtualCamera;

        if (activeCamera) {
            foreach (var element in m_elements) {
                if (activeCamera) {
                    Vector3 parallaxPos = element.initialPosition + ((activeCamera.transform.position - transform.position) * m_parallaxFactor);
                    element.transform.localPosition = new Vector3(parallaxPos.x, parallaxPos.y, element.transform.localPosition.z);
                }
            }
        }
    }
}
