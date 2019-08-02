using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class AreaTransition : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private int m_transitionId = 0;
    [SerializeField] private Area m_transitionArea = default;
    [SerializeField] private Transform m_spawnPoint = default;

    //******************//
    //    Properties    //
    //******************//

    public AreaController controller { set { m_controller = value; } }
    public int transitionId { get { return m_transitionId; } }
    public Transform spawnPoint { get { return m_spawnPoint; } }

    //**********************//
    //    Private Fields    //
    //**********************//

    private AreaController m_controller = default;
    private AreaTransitionManager manager;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        manager = AreaTransitionManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            manager.Transition(m_controller.area, m_transitionArea, m_transitionId);
        }
    }
}
