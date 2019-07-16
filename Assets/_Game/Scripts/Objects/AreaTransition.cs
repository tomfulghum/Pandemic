using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]

public class AreaTransition : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private AreaController m_controller = default;
    [SerializeField] private int m_id = 0;
    [SerializeField] private Area m_area = default;
    [SerializeField] private Transform m_spawnPoint = default;

    //******************//
    //    Properties    //
    //******************//

    public int id { get { return m_id; } }
    public Transform spawnPoint { get { return m_spawnPoint; } }

    //**********************//
    //    Private Fields    //
    //**********************//

    private AreaTransitionManager manager;
    private bool transitioning = false;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Start()
    {
        manager = AreaTransitionManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!transitioning) {
            manager.Transition(m_controller.area, m_area, m_id);
            transitioning = true;
        }
    }
}
