using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTester : MonoBehaviour
{
    [SerializeField] private GameObject player = default;
    [SerializeField] private int transitionId = 0;

    private void Start()
    {
        AreaController controller = FindObjectOfType<AreaController>();
        controller.InitializeArea(player, transitionId);
    }
}
