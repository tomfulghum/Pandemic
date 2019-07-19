using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AreaController : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private Area m_area = default;
    [SerializeField] private List<AreaTransition> m_areaTransitions = default;
    [SerializeField] private GameObject m_playerFollowCamera = default;

    //******************//
    //    Properties    //
    //******************//

    public Area area { get { return m_area; } }

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void OnValidate()
    {
        RegisterWithTransitions();
    }

    private void Start()
    {
        RegisterWithTransitions();
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private void RegisterWithTransitions()
    {
        foreach (var transition in m_areaTransitions) {
            if (transition) {
                transition.controller = this;
            }
        }
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void InitializeArea(GameObject _player, int _transitionId)
    {
        AreaTransition transition = m_areaTransitions.Find(x => x.transitionId == _transitionId);
        if (transition) {
            _player.transform.position = transition.spawnPoint.position;
            CinemachineVirtualCamera cam = Instantiate(m_playerFollowCamera, _player.transform.position, Quaternion.identity).GetComponent<CinemachineVirtualCamera>();
            cam.Follow = _player.transform;
        } else {
            Debug.LogError("[AreaController] Invalid transition ID! (" + _transitionId + ")");
        }
    }
}
