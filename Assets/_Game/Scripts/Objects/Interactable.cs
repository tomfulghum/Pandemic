using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlayerInput))]

public class Interactable : MonoBehaviour
{
    //**********************//
    //    Internal Types    //
    //**********************//

    public delegate bool ConditionDelegate();

    //**********************//
    //   Inspector Fields   //
    //**********************//

    [Tooltip("The main effect(s) this interactable causes when interacted with. This is also called when an objects state is set by the save state loader.")]
    [SerializeField] private UnityEvent m_onInteraction = default;
    [Tooltip("All side effects caused by interaction. This is not called when the state is set externally.")]
    [SerializeField] private UnityEvent m_onInteractionSideEffects = default;

    //******************//
    //    Properties    //
    //******************//

    public GameObject player
    {
        get { return m_player; }
    }

    //*********************//
    //    Public Fields    //
    //*********************//

    public ConditionDelegate conditionDelegate;

    //**********************//
    //    Private Fields    //
    //**********************//

    private PlayerInput m_input;
    private GameObject m_player;

    //*******************************//
    //    MonoBehaviour Functions    //
    //*******************************//

    private void Awake()
    {
        m_input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (m_player && m_input.player.GetButtonDown(m_input.interactButton)) {
            bool conditionMet = true;

            if (conditionDelegate != null) {
                var invocationList = conditionDelegate.GetInvocationList();
                foreach (ConditionDelegate invocation in invocationList) {
                    if (!invocation.Invoke()) {
                        conditionMet = false;
                        break;
                    }
                }
            }

            if (conditionMet) {
                m_onInteractionSideEffects?.Invoke();
                m_onInteraction?.Invoke();
            }
        }  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            m_player = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            m_player = null;
        }
    }

    //************************//
    //    Public Functions    //
    //************************//

    public void SimulateInteraction()
    {
        m_onInteraction?.Invoke();
    }
}
