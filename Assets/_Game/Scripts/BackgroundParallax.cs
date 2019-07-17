using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(SpriteRenderer))]

public class BackgroundParallax : MonoBehaviour
{
    [SerializeField] private float parallaxFactor = 0f;

    private float lastCameraPosition = 0;

    private void Update()
    {
        CinemachineVirtualCamera activeCamera = (CinemachineVirtualCamera)Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

        if (activeCamera) {
            if (lastCameraPosition != 0) {
                transform.position += Vector3.left * (lastCameraPosition - activeCamera.transform.position.x) * parallaxFactor;
            }

            lastCameraPosition = activeCamera.transform.position.x;
        }
    }
}
