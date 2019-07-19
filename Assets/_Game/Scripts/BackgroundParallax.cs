using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BackgroundParallax : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private float parallaxFactor = 0f;

    //**********************//
    //    Private Fields    //
    //**********************//

    private List<BackgroundParallaxElement> elements = new List<BackgroundParallaxElement>();

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        foreach (Transform child in transform) {
            var element = child.GetComponent<BackgroundParallaxElement>();
            if (element) {
                elements.Add(element);
            }
        }
    }

    private void Update()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera) {
            CinemachineVirtualCamera activeCamera = (CinemachineVirtualCamera)mainCamera.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

            if (activeCamera) {
                foreach (var element in elements) {
                    if (activeCamera) {
                        Vector3 parallaxPos = element.initialPosition + ((activeCamera.transform.position - transform.position) * parallaxFactor);
                        element.transform.localPosition = new Vector3(parallaxPos.x, parallaxPos.y, element.transform.localPosition.z);
                    }
                }
            }
        }
    }
}
