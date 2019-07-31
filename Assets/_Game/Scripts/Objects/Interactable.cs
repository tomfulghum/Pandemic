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

    [SerializeField] private UnityEvent m_onInteraction = default;

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
}
