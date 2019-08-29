using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTester : MonoBehaviour
{
    [SerializeField] private GameObject player = default;

    private void Start()
    {
        AreaController controller = FindObjectOfType<AreaController>();
        controller.InitializeArea(player, controller.area.spawnPoints[0]);
    }
}
