using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class AreaTransition : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private SpawnPointData m_transitionSpawnPoint;

    //**********************//
    //    Private Fields    //
    //**********************//

    private AreaController m_controller = default;
    private AreaTransitionManager m_manager = default;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        m_controller = FindObjectOfType<AreaController>();
        m_manager = AreaTransitionManager.Instance;

        if (!m_controller) {
            Debug.LogErrorFormat("{0}: Could not find area controller!", name);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            m_manager.Transition(m_controller.area, m_transitionSpawnPoint);
        }
    }
}
