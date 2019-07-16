using System.Collections.Generic;
using UnityEngine;

public class AreaController : MonoBehaviour
{
    //************************//
    //    Inspector Fields    //
    //************************//

    [SerializeField] private Area m_area = default;
    [SerializeField] private List<AreaTransition> m_areaTransitions = default;

    //******************//
    //    Properties    //
    //******************//

    public Area area { get { return m_area; } }

    //************************//
    //    Public Functions    //
    //************************//

    public AreaTransition GetTransition(int _id)
    {
        return m_areaTransitions.Find(x => x.id == _id);
    }
}
