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
    [SerializeField] private bool m_oneTimeUse = true;

    //**********************//
    //    Private Fields    //
    //**********************//

    private LeverState m_state;
    private Interactable m_interactable;
    private Animator m_anim;
    private int m_animParamPulled;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_state = new LeverState();
        m_interactable = GetComponent<Interactable>();
        m_anim = GetComponent<Animator>();
        m_animParamPulled = Animator.StringToHash("Pulled");
    }

    private void Start()
    {
        m_interactable.conditionDelegate += CheckCondition;
    }

    private void OnDisable()
    {
        m_interactable.conditionDelegate -= CheckCondition;
    }

    //*************************//
    //    Private Functions    //
    //*************************//

    private bool CheckCondition()
    {
        return m_interactable.player && m_interactable.player.GetComponent<PlayerInventory>().normalKeyCount >= m_requiredKeyCount;
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void Use()
    {
        m_anim.SetBool(m_animParamPulled, !m_anim.GetBool(m_animParamPulled));

        if (m_oneTimeUse) {
            m_state.used = true;
            m_interactable.enabled = false;
        }
    }

    public void RemoveKeysFromPlayer()
    {
        if (m_interactable.player) {
            m_interactable.player.GetComponent<PlayerInventory>().normalKeyCount -= m_requiredKeyCount;
        }
    }

    public void SetState(LeverState _state)
    {
        m_state = _state;
        if (m_state.used) {
            m_interactable.SimulateInteraction();
        }
    }

    public LeverState GetState()
    {
        return m_state;
    }
}
