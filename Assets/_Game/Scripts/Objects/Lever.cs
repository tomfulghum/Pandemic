using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]

public class Lever : MonoBehaviour
{
    //**********************//
    //   Inspector Fields   //
    //**********************//

    [SerializeField] private int m_requiredKeyCount = 0;

    //**********************//
    //    Private Fields    //
    //**********************//

    private Interactable m_interactable;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_interactable = GetComponent<Interactable>();
    }

    private void Start()
    {
        m_interactable.conditionDelegate += CheckCondition;
    }

    private void OnDisable()
    {
        m_interactable.conditionDelegate -= CheckCondition;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public bool CheckCondition()
    {
        return m_interactable.player && m_interactable.player.GetComponent<PlayerInventory>().normalKeyCount >= m_requiredKeyCount;
    }
}
