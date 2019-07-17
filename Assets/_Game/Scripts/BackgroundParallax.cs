using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(SpriteRenderer))]

public class BackgroundParallax : MonoBehaviour
{
    [SerializeField] private float parallaxFactor = 0f;

    private Vector2 lastCameraPosition = Vector2.zero;

    private void Update()
    {
        CinemachineVirtualCamera activeCamera = (CinemachineVirtualCamera)Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

        if (activeCamera) {
            if (lastCameraPosition != Vector2.zero) {
                transform.position += (Vector3)((Vector2)activeCamera.transform.position - lastCameraPosition) * parallaxFactor;
            }

            lastCameraPosition = activeCamera.transform.position;
        }
    }
}
